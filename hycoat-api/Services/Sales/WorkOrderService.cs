using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Sales;
using HycoatApi.Models.Sales;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Sales;

public class WorkOrderService : IWorkOrderService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    private static readonly Dictionary<string, string[]> StatusTransitions = new()
    {
        ["Created"] = ["MaterialAwaited"],
        ["MaterialAwaited"] = ["MaterialReceived"],
        ["MaterialReceived"] = ["InProduction"],
        ["InProduction"] = ["QAComplete"],
        ["QAComplete"] = ["Dispatched"],
        ["Dispatched"] = ["Invoiced"],
        ["Invoiced"] = ["Closed"],
    };

    public WorkOrderService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResponse<WorkOrderDto>> GetAllAsync(
        string? search, string? status, int? customerId,
        int? processTypeId, int? powderColorId,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.WorkOrders
            .AsNoTracking()
            .Include(wo => wo.Customer)
            .Include(wo => wo.ProcessType)
            .Include(wo => wo.PowderColor)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(wo =>
                wo.WONumber.ToLower().Contains(term) ||
                wo.Customer.Name.ToLower().Contains(term) ||
                (wo.ProjectName != null && wo.ProjectName.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(wo => wo.Status == status);

        if (customerId.HasValue)
            query = query.Where(wo => wo.CustomerId == customerId.Value);

        if (processTypeId.HasValue)
            query = query.Where(wo => wo.ProcessTypeId == processTypeId.Value);

        if (powderColorId.HasValue)
            query = query.Where(wo => wo.PowderColorId == powderColorId.Value);

        if (fromDate.HasValue)
            query = query.Where(wo => wo.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(wo => wo.Date <= toDate.Value);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "wonumber" => sortDesc ? query.OrderByDescending(wo => wo.WONumber) : query.OrderBy(wo => wo.WONumber),
            "customer" or "customername" => sortDesc ? query.OrderByDescending(wo => wo.Customer.Name) : query.OrderBy(wo => wo.Customer.Name),
            "status" => sortDesc ? query.OrderByDescending(wo => wo.Status) : query.OrderBy(wo => wo.Status),
            "dispatchdate" => sortDesc ? query.OrderByDescending(wo => wo.DispatchDate) : query.OrderBy(wo => wo.DispatchDate),
            _ => sortDesc ? query.OrderByDescending(wo => wo.Date) : query.OrderBy(wo => wo.Date),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(wo => new WorkOrderDto
            {
                Id = wo.Id,
                WONumber = wo.WONumber,
                Date = wo.Date,
                CustomerName = wo.Customer.Name,
                ProjectName = wo.ProjectName,
                ProcessTypeName = wo.ProcessType.Name,
                PowderColorName = wo.PowderColor != null ? wo.PowderColor.ColorName : null,
                PowderCode = wo.PowderColor != null ? wo.PowderColor.PowderCode : null,
                Status = wo.Status,
                DispatchDate = wo.DispatchDate
            })
            .ToListAsync();

        return new PagedResponse<WorkOrderDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<WorkOrderDetailDto> GetByIdAsync(int id)
    {
        var wo = await _db.WorkOrders
            .AsNoTracking()
            .Include(w => w.Customer)
            .Include(w => w.ProcessType)
            .Include(w => w.PowderColor)
            .Include(w => w.ProformaInvoice)
            .Include(w => w.MaterialInwards)
            .Include(w => w.ProductionWorkOrders)
            .Include(w => w.DeliveryChallans)
            .Include(w => w.Invoices)
            .FirstOrDefaultAsync(w => w.Id == id)
            ?? throw new KeyNotFoundException($"Work Order with ID {id} not found.");

        return _mapper.Map<WorkOrderDetailDto>(wo);
    }

    public async Task<WorkOrderDto> CreateAsync(CreateWorkOrderDto dto, string userId)
    {
        var customerExists = await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId);
        if (!customerExists)
            throw new ArgumentException("Customer not found.");

        var ptExists = await _db.ProcessTypes.AnyAsync(pt => pt.Id == dto.ProcessTypeId);
        if (!ptExists)
            throw new ArgumentException("Process type not found.");

        if (dto.PowderColorId.HasValue)
        {
            var colorExists = await _db.PowderColors.AnyAsync(pc => pc.Id == dto.PowderColorId.Value);
            if (!colorExists)
                throw new ArgumentException("Powder color not found.");
        }

        if (dto.ProformaInvoiceId.HasValue)
        {
            var piExists = await _db.ProformaInvoices.AnyAsync(pi => pi.Id == dto.ProformaInvoiceId.Value);
            if (!piExists)
                throw new ArgumentException("Proforma Invoice not found.");
        }

        var wo = _mapper.Map<WorkOrder>(dto);
        wo.WONumber = await GenerateNumberAsync();
        wo.Status = "Created";
        wo.CreatedBy = userId;

        _db.WorkOrders.Add(wo);

        // If linked to a PI → Quotation → Inquiry chain, update inquiry to Confirmed
        if (dto.ProformaInvoiceId.HasValue)
        {
            var pi = await _db.ProformaInvoices
                .Include(p => p.Quotation)
                .FirstOrDefaultAsync(p => p.Id == dto.ProformaInvoiceId.Value);

            if (pi?.Quotation?.InquiryId != null)
            {
                var inquiry = await _db.Inquiries.FindAsync(pi.Quotation.InquiryId.Value);
                if (inquiry != null && inquiry.Status == "PISent")
                {
                    inquiry.Status = "Confirmed";
                    inquiry.UpdatedBy = userId;
                    inquiry.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await _db.SaveChangesAsync();

        var created = await _db.WorkOrders
            .AsNoTracking()
            .Include(w => w.Customer)
            .Include(w => w.ProcessType)
            .Include(w => w.PowderColor)
            .FirstAsync(w => w.Id == wo.Id);

        return _mapper.Map<WorkOrderDto>(created);
    }

    public async Task<WorkOrderDto> UpdateAsync(int id, CreateWorkOrderDto dto, string userId)
    {
        var wo = await _db.WorkOrders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Work Order with ID {id} not found.");

        // Block customer/process changes if material has been received
        if (wo.Status != "Created" && wo.Status != "MaterialAwaited")
        {
            if (wo.CustomerId != dto.CustomerId || wo.ProcessTypeId != dto.ProcessTypeId)
                throw new InvalidOperationException("Cannot change customer or process type after material has been received.");
        }

        var customerExists = await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId);
        if (!customerExists)
            throw new ArgumentException("Customer not found.");

        var ptExists = await _db.ProcessTypes.AnyAsync(pt => pt.Id == dto.ProcessTypeId);
        if (!ptExists)
            throw new ArgumentException("Process type not found.");

        if (dto.PowderColorId.HasValue)
        {
            var colorExists = await _db.PowderColors.AnyAsync(pc => pc.Id == dto.PowderColorId.Value);
            if (!colorExists)
                throw new ArgumentException("Powder color not found.");
        }

        wo.Date = dto.Date;
        wo.CustomerId = dto.CustomerId;
        wo.ProformaInvoiceId = dto.ProformaInvoiceId;
        wo.ProjectName = dto.ProjectName;
        wo.ProcessTypeId = dto.ProcessTypeId;
        wo.PowderColorId = dto.PowderColorId;
        wo.DispatchDate = dto.DispatchDate;
        wo.Notes = dto.Notes;
        wo.UpdatedBy = userId;
        wo.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var updated = await _db.WorkOrders
            .AsNoTracking()
            .Include(w => w.Customer)
            .Include(w => w.ProcessType)
            .Include(w => w.PowderColor)
            .FirstAsync(w => w.Id == id);

        return _mapper.Map<WorkOrderDto>(updated);
    }

    public async Task UpdateStatusAsync(int id, UpdateWorkOrderStatusDto dto, string userId)
    {
        var wo = await _db.WorkOrders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Work Order with ID {id} not found.");

        if (!StatusTransitions.TryGetValue(wo.Status, out var allowed) || !allowed.Contains(dto.Status))
            throw new InvalidOperationException(
                $"Cannot transition from '{wo.Status}' to '{dto.Status}'.");

        wo.Status = dto.Status;
        wo.UpdatedBy = userId;
        wo.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var wo = await _db.WorkOrders
            .Include(w => w.MaterialInwards)
            .Include(w => w.ProductionWorkOrders)
            .Include(w => w.DeliveryChallans)
            .Include(w => w.Invoices)
            .FirstOrDefaultAsync(w => w.Id == id)
            ?? throw new KeyNotFoundException($"Work Order with ID {id} not found.");

        // Block delete if downstream documents exist
        if (wo.MaterialInwards.Any() || wo.ProductionWorkOrders.Any() ||
            wo.DeliveryChallans.Any() || wo.Invoices.Any())
            throw new InvalidOperationException("Cannot delete a work order with linked documents.");

        wo.IsDeleted = true;
        wo.UpdatedBy = userId;
        wo.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<WorkOrderStatsDto> GetStatsAsync()
    {
        var counts = await _db.WorkOrders
            .AsNoTracking()
            .GroupBy(wo => wo.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        return new WorkOrderStatsDto
        {
            Created = counts.FirstOrDefault(c => c.Status == "Created")?.Count ?? 0,
            MaterialAwaited = counts.FirstOrDefault(c => c.Status == "MaterialAwaited")?.Count ?? 0,
            MaterialReceived = counts.FirstOrDefault(c => c.Status == "MaterialReceived")?.Count ?? 0,
            InProduction = counts.FirstOrDefault(c => c.Status == "InProduction")?.Count ?? 0,
            QAComplete = counts.FirstOrDefault(c => c.Status == "QAComplete")?.Count ?? 0,
            Dispatched = counts.FirstOrDefault(c => c.Status == "Dispatched")?.Count ?? 0,
            Invoiced = counts.FirstOrDefault(c => c.Status == "Invoiced")?.Count ?? 0,
            Closed = counts.FirstOrDefault(c => c.Status == "Closed")?.Count ?? 0,
            Total = counts.Sum(c => c.Count)
        };
    }

    public async Task<WorkOrderTimelineDto> GetTimelineAsync(int id)
    {
        var wo = await _db.WorkOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id)
            ?? throw new KeyNotFoundException($"Work Order with ID {id} not found.");

        var timeline = new WorkOrderTimelineDto
        {
            WorkOrderId = wo.Id,
            WONumber = wo.WONumber
        };

        // 1. Created
        timeline.Events.Add(new TimelineEventDto
        {
            Stage = "Created",
            DocumentNumber = wo.WONumber,
            Date = wo.CreatedAt,
            IsComplete = true
        });

        // 2. Material Received
        var materialInward = await _db.MaterialInwards
            .AsNoTracking()
            .Where(m => m.WorkOrderId == id)
            .OrderByDescending(m => m.Date)
            .Select(m => new { m.InwardNumber, m.Date })
            .FirstOrDefaultAsync();

        timeline.Events.Add(new TimelineEventDto
        {
            Stage = "MaterialReceived",
            DocumentNumber = materialInward?.InwardNumber,
            Date = materialInward?.Date,
            IsComplete = materialInward != null
        });

        // 3. In Production
        var pwo = await _db.ProductionWorkOrders
            .AsNoTracking()
            .Where(p => p.WorkOrderId == id)
            .OrderByDescending(p => p.Date)
            .Select(p => new { p.PWONumber, p.Date })
            .FirstOrDefaultAsync();

        timeline.Events.Add(new TimelineEventDto
        {
            Stage = "InProduction",
            DocumentNumber = pwo?.PWONumber,
            Date = pwo?.Date,
            IsComplete = pwo != null
        });

        // 4. QA Complete
        var finalInspection = await _db.ProductionWorkOrders
            .AsNoTracking()
            .Where(p => p.WorkOrderId == id)
            .SelectMany(p => _db.FinalInspections
                .Where(fi => fi.ProductionWorkOrderId == p.Id && fi.OverallStatus == "Approved"))
            .OrderByDescending(fi => fi.Date)
            .Select(fi => new { fi.InspectionNumber, fi.Date })
            .FirstOrDefaultAsync();

        timeline.Events.Add(new TimelineEventDto
        {
            Stage = "QAComplete",
            DocumentNumber = finalInspection?.InspectionNumber,
            Date = finalInspection?.Date,
            IsComplete = finalInspection != null
        });

        // 5. Dispatched
        var dc = await _db.DeliveryChallans
            .AsNoTracking()
            .Where(d => d.WorkOrderId == id)
            .OrderByDescending(d => d.Date)
            .Select(d => new { d.DCNumber, d.Date })
            .FirstOrDefaultAsync();

        timeline.Events.Add(new TimelineEventDto
        {
            Stage = "Dispatched",
            DocumentNumber = dc?.DCNumber,
            Date = dc?.Date,
            IsComplete = dc != null
        });

        // 6. Invoiced
        var invoice = await _db.Invoices
            .AsNoTracking()
            .Where(inv => inv.WorkOrderId == id)
            .OrderByDescending(inv => inv.Date)
            .Select(inv => new { inv.InvoiceNumber, inv.Date })
            .FirstOrDefaultAsync();

        timeline.Events.Add(new TimelineEventDto
        {
            Stage = "Invoiced",
            DocumentNumber = invoice?.InvoiceNumber,
            Date = invoice?.Date,
            IsComplete = invoice != null
        });

        // 7. Closed
        timeline.Events.Add(new TimelineEventDto
        {
            Stage = "Closed",
            Date = wo.Status == "Closed" ? wo.UpdatedAt : null,
            IsComplete = wo.Status == "Closed"
        });

        return timeline;
    }

    public async Task<List<LookupDto>> GetLookupAsync()
    {
        return await _db.WorkOrders
            .AsNoTracking()
            .Where(wo => wo.Status != "Closed")
            .OrderByDescending(wo => wo.Date)
            .Select(wo => new LookupDto { Id = wo.Id, Name = wo.WONumber })
            .ToListAsync();
    }

    private async Task<string> GenerateNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"WO-{year}-";

        var lastNumber = await _db.WorkOrders
            .Where(wo => wo.WONumber.StartsWith(prefix))
            .OrderByDescending(wo => wo.WONumber)
            .Select(wo => wo.WONumber)
            .FirstOrDefaultAsync();

        var nextSeq = 1;
        if (lastNumber != null)
        {
            var seqPart = lastNumber.Replace(prefix, "");
            if (int.TryParse(seqPart, out var seq))
                nextSeq = seq + 1;
        }
        return $"{prefix}{nextSeq:D3}";
    }
}

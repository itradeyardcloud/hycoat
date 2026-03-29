using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Purchase;
using HycoatApi.Models.Purchase;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Purchase;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly PurchaseOrderPdfService _pdfService;

    public PurchaseOrderService(AppDbContext db, IMapper mapper, PurchaseOrderPdfService pdfService)
    {
        _db = db;
        _mapper = mapper;
        _pdfService = pdfService;
    }

    public async Task<PagedResponse<PurchaseOrderDto>> GetAllAsync(
        string? search, string? status, int? vendorId, DateTime? dateFrom, DateTime? dateTo,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.PurchaseOrders
            .AsNoTracking()
            .Include(po => po.Vendor)
            .Include(po => po.PowderIndent)
            .Include(po => po.LineItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(po =>
                po.PONumber.ToLower().Contains(term) ||
                po.Vendor.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(po => po.Status == status);

        if (vendorId.HasValue)
            query = query.Where(po => po.VendorId == vendorId.Value);

        if (dateFrom.HasValue)
            query = query.Where(po => po.Date >= dateFrom.Value.Date);

        if (dateTo.HasValue)
            query = query.Where(po => po.Date <= dateTo.Value.Date);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "ponumber" => sortDesc
                ? query.OrderByDescending(po => po.PONumber)
                : query.OrderBy(po => po.PONumber),
            "vendor" => sortDesc
                ? query.OrderByDescending(po => po.Vendor.Name)
                : query.OrderBy(po => po.Vendor.Name),
            "status" => sortDesc
                ? query.OrderByDescending(po => po.Status)
                : query.OrderBy(po => po.Status),
            _ => sortDesc
                ? query.OrderByDescending(po => po.Date).ThenByDescending(po => po.Id)
                : query.OrderBy(po => po.Date).ThenBy(po => po.Id),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<PurchaseOrderDto>
        {
            Items = _mapper.Map<List<PurchaseOrderDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PurchaseOrderDetailDto> GetByIdAsync(int id)
    {
        var po = await _db.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Include(p => p.PowderIndent)
            .Include(p => p.LineItems).ThenInclude(l => l.PowderColor)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Purchase Order with ID {id} not found.");

        return _mapper.Map<PurchaseOrderDetailDto>(po);
    }

    public async Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto, string userId)
    {
        var vendor = await _db.Vendors.FindAsync(dto.VendorId)
            ?? throw new ArgumentException("Vendor not found.");

        if (dto.PowderIndentId.HasValue)
        {
            var indent = await _db.PowderIndents.FindAsync(dto.PowderIndentId.Value)
                ?? throw new ArgumentException("Powder Indent not found.");

            if (indent.Status != "Approved" && indent.Status != "Ordered")
                throw new InvalidOperationException("Indent must be in 'Approved' or 'Ordered' status to create a PO.");
        }

        var po = _mapper.Map<PurchaseOrder>(dto);
        po.PONumber = await GenerateNumberAsync();
        po.Date = dto.Date.Date;
        po.Status = "Draft";
        po.CreatedBy = userId;

        foreach (var lineDto in dto.Lines)
        {
            var line = _mapper.Map<POLineItem>(lineDto);
            line.Amount = Math.Round(line.QtyKg * line.RatePerKg, 2);
            po.LineItems.Add(line);
        }

        _db.PurchaseOrders.Add(po);

        // Update indent status to Ordered if linked
        if (dto.PowderIndentId.HasValue)
        {
            var indent = await _db.PowderIndents.FindAsync(dto.PowderIndentId.Value);
            if (indent != null && indent.Status == "Approved")
            {
                indent.Status = "Ordered";
                indent.UpdatedBy = userId;
                indent.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();

        var created = await _db.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Include(p => p.PowderIndent)
            .Include(p => p.LineItems)
            .FirstAsync(p => p.Id == po.Id);

        return _mapper.Map<PurchaseOrderDto>(created);
    }

    public async Task<PurchaseOrderDto> UpdateAsync(int id, CreatePurchaseOrderDto dto, string userId)
    {
        var po = await _db.PurchaseOrders
            .Include(p => p.LineItems)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Purchase Order with ID {id} not found.");

        if (po.Status != "Draft")
            throw new InvalidOperationException("Only Purchase Orders with 'Draft' status can be updated.");

        po.Date = dto.Date.Date;
        po.VendorId = dto.VendorId;
        po.PowderIndentId = dto.PowderIndentId;
        po.Notes = dto.Notes;
        po.UpdatedBy = userId;
        po.UpdatedAt = DateTime.UtcNow;

        po.LineItems.Clear();
        foreach (var lineDto in dto.Lines)
        {
            var line = _mapper.Map<POLineItem>(lineDto);
            line.Amount = Math.Round(line.QtyKg * line.RatePerKg, 2);
            po.LineItems.Add(line);
        }

        await _db.SaveChangesAsync();

        var updated = await _db.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Include(p => p.PowderIndent)
            .Include(p => p.LineItems)
            .FirstAsync(p => p.Id == id);

        return _mapper.Map<PurchaseOrderDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var po = await _db.PurchaseOrders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Purchase Order with ID {id} not found.");

        po.IsDeleted = true;
        po.UpdatedBy = userId;
        po.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<PurchaseOrderDto> UpdateStatusAsync(int id, UpdatePurchaseOrderStatusDto dto, string userId)
    {
        var po = await _db.PurchaseOrders
            .Include(p => p.LineItems)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Purchase Order with ID {id} not found.");

        ValidateStatusTransition(po.Status, dto.Status);

        po.Status = dto.Status;
        po.UpdatedBy = userId;
        po.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var updated = await _db.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Include(p => p.PowderIndent)
            .Include(p => p.LineItems)
            .FirstAsync(p => p.Id == id);

        return _mapper.Map<PurchaseOrderDto>(updated);
    }

    public async Task<byte[]> GeneratePdfAsync(int id)
    {
        var po = await _db.PurchaseOrders
            .AsNoTracking()
            .Include(p => p.Vendor)
            .Include(p => p.PowderIndent)
            .Include(p => p.LineItems).ThenInclude(l => l.PowderColor)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Purchase Order with ID {id} not found.");

        var detail = _mapper.Map<PurchaseOrderDetailDto>(po);
        return _pdfService.Generate(detail);
    }

    private static void ValidateStatusTransition(string currentStatus, string newStatus)
    {
        var allowed = currentStatus switch
        {
            "Draft" => new[] { "Sent", "Cancelled" },
            "Sent" => new[] { "PartiallyReceived", "Received", "Cancelled" },
            "PartiallyReceived" => new[] { "Received", "Cancelled" },
            _ => Array.Empty<string>()
        };

        if (!allowed.Contains(newStatus))
            throw new InvalidOperationException(
                $"Cannot transition from '{currentStatus}' to '{newStatus}'.");
    }

    private async Task<string> GenerateNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"PO-{year}-";

        var lastNumber = await _db.PurchaseOrders
            .Where(po => po.PONumber.StartsWith(prefix))
            .OrderByDescending(po => po.PONumber)
            .Select(po => po.PONumber)
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

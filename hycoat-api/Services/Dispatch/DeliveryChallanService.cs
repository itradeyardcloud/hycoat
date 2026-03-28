using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Dispatch;
using HycoatApi.Models.Common;
using HycoatApi.Models.Dispatch;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Dispatch;

public class DeliveryChallanService : IDeliveryChallanService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;

    public DeliveryChallanService(AppDbContext db, IMapper mapper, IWebHostEnvironment env)
    {
        _db = db;
        _mapper = mapper;
        _env = env;
    }

    public async Task<PagedResponse<DeliveryChallanDto>> GetAllAsync(
        string? search, DateTime? date, int? customerId, int? workOrderId, string? status,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.DeliveryChallans
            .AsNoTracking()
            .Include(d => d.Customer)
            .Include(d => d.WorkOrder)
            .Include(d => d.LineItems)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(d =>
                d.DCNumber.ToLower().Contains(term) ||
                d.Customer.Name.ToLower().Contains(term) ||
                (d.VehicleNumber != null && d.VehicleNumber.ToLower().Contains(term)));
        }

        if (date.HasValue)
            query = query.Where(d => d.Date == date.Value.Date);

        if (customerId.HasValue)
            query = query.Where(d => d.CustomerId == customerId.Value);

        if (workOrderId.HasValue)
            query = query.Where(d => d.WorkOrderId == workOrderId.Value);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(d => d.Status == status);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "dcnumber" => sortDesc
                ? query.OrderByDescending(d => d.DCNumber)
                : query.OrderBy(d => d.DCNumber),
            "customer" => sortDesc
                ? query.OrderByDescending(d => d.Customer.Name)
                : query.OrderBy(d => d.Customer.Name),
            "status" => sortDesc
                ? query.OrderByDescending(d => d.Status)
                : query.OrderBy(d => d.Status),
            _ => sortDesc
                ? query.OrderByDescending(d => d.Date)
                : query.OrderBy(d => d.Date),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<DeliveryChallanDto>
        {
            Items = _mapper.Map<List<DeliveryChallanDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<DeliveryChallanDetailDto> GetByIdAsync(int id)
    {
        var dc = await _db.DeliveryChallans
            .AsNoTracking()
            .Include(d => d.Customer)
            .Include(d => d.WorkOrder)
            .Include(d => d.LineItems).ThenInclude(l => l.SectionProfile)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException($"Delivery Challan with ID {id} not found.");

        var dto = _mapper.Map<DeliveryChallanDetailDto>(dc);

        // Load loading photo URLs
        var photos = await _db.FileAttachments
            .AsNoTracking()
            .Where(f => f.EntityType == "DeliveryChallan" && f.EntityId == id && !f.IsDeleted)
            .Select(f => f.StoredPath)
            .ToListAsync();
        dto.LoadingPhotoUrls = photos;

        return dto;
    }

    public async Task<DeliveryChallanDto> CreateAsync(CreateDeliveryChallanDto dto, string userId)
    {
        var workOrder = await _db.WorkOrders.FindAsync(dto.WorkOrderId)
            ?? throw new ArgumentException("Work Order not found.");

        var customer = await _db.Customers.FindAsync(dto.CustomerId)
            ?? throw new ArgumentException("Customer not found.");

        // Validate final inspection is Approved for this WO
        var hasApprovedFI = await _db.FinalInspections
            .Include(f => f.ProductionWorkOrder)
            .AnyAsync(f => f.ProductionWorkOrder.WorkOrderId == dto.WorkOrderId
                        && f.OverallStatus == "Approved");

        if (!hasApprovedFI)
            throw new InvalidOperationException("Cannot create Delivery Challan: Final Inspection must be Approved for this Work Order.");

        // Auto-generate DC number
        var year = DateTime.UtcNow.Year;
        var lastNumber = await _db.DeliveryChallans
            .Where(d => d.DCNumber.StartsWith($"DC-{year}-"))
            .OrderByDescending(d => d.DCNumber)
            .Select(d => d.DCNumber)
            .FirstOrDefaultAsync();

        var sequence = 1;
        if (lastNumber != null)
        {
            var parts = lastNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var lastSeq))
                sequence = lastSeq + 1;
        }

        var dc = _mapper.Map<DeliveryChallan>(dto);
        dc.DCNumber = $"DC-{year}-{sequence:D3}";
        dc.Date = dto.Date.Date;
        dc.Status = "Created";
        dc.CreatedBy = userId;

        // Snapshot customer data
        dc.CustomerAddress = customer.Address;
        dc.CustomerGSTIN = customer.GSTIN;

        dc.LineItems = _mapper.Map<List<DCLineItem>>(dto.Lines);

        _db.DeliveryChallans.Add(dc);
        await _db.SaveChangesAsync();

        var created = await _db.DeliveryChallans
            .AsNoTracking()
            .Include(d => d.Customer)
            .Include(d => d.WorkOrder)
            .Include(d => d.LineItems)
            .FirstAsync(d => d.Id == dc.Id);

        return _mapper.Map<DeliveryChallanDto>(created);
    }

    public async Task<DeliveryChallanDto> UpdateAsync(int id, CreateDeliveryChallanDto dto, string userId)
    {
        var dc = await _db.DeliveryChallans
            .Include(d => d.LineItems)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException($"Delivery Challan with ID {id} not found.");

        if (dc.Status != "Created")
            throw new InvalidOperationException("Only Delivery Challans with status 'Created' can be updated.");

        dc.Date = dto.Date.Date;
        dc.WorkOrderId = dto.WorkOrderId;
        dc.CustomerId = dto.CustomerId;
        dc.VehicleNumber = dto.VehicleNumber;
        dc.DriverName = dto.DriverName;
        dc.LRNumber = dto.LRNumber;
        dc.MaterialValueApprox = dto.MaterialValueApprox;
        dc.Notes = dto.Notes;
        dc.UpdatedBy = userId;
        dc.UpdatedAt = DateTime.UtcNow;

        // Replace lines
        _db.DCLineItems.RemoveRange(dc.LineItems);
        dc.LineItems = _mapper.Map<List<DCLineItem>>(dto.Lines);

        await _db.SaveChangesAsync();

        var updated = await _db.DeliveryChallans
            .AsNoTracking()
            .Include(d => d.Customer)
            .Include(d => d.WorkOrder)
            .Include(d => d.LineItems)
            .FirstAsync(d => d.Id == id);

        return _mapper.Map<DeliveryChallanDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var dc = await _db.DeliveryChallans.FindAsync(id)
            ?? throw new KeyNotFoundException($"Delivery Challan with ID {id} not found.");

        dc.IsDeleted = true;
        dc.UpdatedBy = userId;
        dc.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<DeliveryChallanDto> UpdateStatusAsync(int id, string status, string userId)
    {
        var dc = await _db.DeliveryChallans
            .Include(d => d.Customer)
            .Include(d => d.WorkOrder)
            .Include(d => d.LineItems)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException($"Delivery Challan with ID {id} not found.");

        var validTransitions = new Dictionary<string, string[]>
        {
            ["Created"] = ["Dispatched"],
            ["Dispatched"] = ["Delivered"],
        };

        if (!validTransitions.TryGetValue(dc.Status, out var allowed) || !allowed.Contains(status))
            throw new InvalidOperationException($"Cannot transition from '{dc.Status}' to '{status}'.");

        dc.Status = status;
        dc.UpdatedBy = userId;
        dc.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<DeliveryChallanDto>(dc);
    }

    public async Task<byte[]> GeneratePdfAsync(int id)
    {
        var dc = await _db.DeliveryChallans
            .AsNoTracking()
            .Include(d => d.Customer)
            .Include(d => d.WorkOrder)
            .Include(d => d.LineItems).ThenInclude(l => l.SectionProfile)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new KeyNotFoundException($"Delivery Challan with ID {id} not found.");

        return DeliveryChallanPdfService.Generate(dc);
    }

    public async Task<byte[]?> GetPdfAsync(int id)
    {
        return await Task.FromResult<byte[]?>(null);
        // PDF is generated on-demand, not stored
    }

    public async Task UploadLoadingPhotosAsync(int id, List<IFormFile> files, string userId)
    {
        var dc = await _db.DeliveryChallans.FindAsync(id)
            ?? throw new KeyNotFoundException($"Delivery Challan with ID {id} not found.");

        // Check existing photo count
        var existingCount = await _db.FileAttachments
            .CountAsync(f => f.EntityType == "DeliveryChallan" && f.EntityId == id && !f.IsDeleted);

        if (existingCount + files.Count > 10)
            throw new InvalidOperationException($"Maximum 10 loading photos per DC. Currently {existingCount}, trying to add {files.Count}.");

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp", "image/gif" };
        const long maxSize = 5 * 1024 * 1024; // 5MB

        foreach (var file in files)
        {
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                throw new ArgumentException($"File '{file.FileName}' is not an allowed image type.");

            if (file.Length > maxSize)
                throw new ArgumentException($"File '{file.FileName}' exceeds the 5MB size limit.");

            var year = DateTime.UtcNow.Year;
            var month = DateTime.UtcNow.Month.ToString("D2");
            var sanitizedFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var storedPath = $"uploads/{year}/{month}/{sanitizedFileName}";
            var fullPath = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), storedPath);

            var dir = Path.GetDirectoryName(fullPath)!;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            var attachment = new FileAttachment
            {
                FileName = file.FileName,
                StoredPath = storedPath,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                EntityType = "DeliveryChallan",
                EntityId = id,
                UploadedBy = userId,
                UploadedAt = DateTime.UtcNow,
                CreatedBy = userId,
            };
            _db.FileAttachments.Add(attachment);
        }

        await _db.SaveChangesAsync();
    }
}

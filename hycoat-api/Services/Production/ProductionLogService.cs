using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Production;
using HycoatApi.Models.Production;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Production;

public class ProductionLogService : IProductionLogService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedImageTypes =
        ["image/jpeg", "image/png", "image/webp", "image/gif"];

    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    private const int MaxPhotosPerLog = 20;

    public ProductionLogService(AppDbContext db, IMapper mapper, IWebHostEnvironment env)
    {
        _db = db;
        _mapper = mapper;
        _env = env;
    }

    public async Task<PagedResponse<ProductionLogDto>> GetAllAsync(
        string? search, DateTime? date, string? shift, int? productionWorkOrderId,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.ProductionLogs
            .AsNoTracking()
            .Include(l => l.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(l => l.SupervisorUser)
            .Include(l => l.Photos)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(l =>
                l.ProductionWorkOrder.PWONumber.ToLower().Contains(term) ||
                l.ProductionWorkOrder.Customer.Name.ToLower().Contains(term));
        }

        if (date.HasValue)
            query = query.Where(l => l.Date == date.Value.Date);

        if (!string.IsNullOrWhiteSpace(shift))
            query = query.Where(l => l.Shift == shift);

        if (productionWorkOrderId.HasValue)
            query = query.Where(l => l.ProductionWorkOrderId == productionWorkOrderId.Value);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "pwonumber" or "pwo" => sortDesc
                ? query.OrderByDescending(l => l.ProductionWorkOrder.PWONumber)
                : query.OrderBy(l => l.ProductionWorkOrder.PWONumber),
            "shift" => sortDesc
                ? query.OrderByDescending(l => l.Shift)
                : query.OrderBy(l => l.Shift),
            "oventemperature" or "temp" => sortDesc
                ? query.OrderByDescending(l => l.OvenTemperature)
                : query.OrderBy(l => l.OvenTemperature),
            _ => sortDesc
                ? query.OrderByDescending(l => l.Date)
                : query.OrderBy(l => l.Date),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<ProductionLogDto>
        {
            Items = _mapper.Map<List<ProductionLogDto>>(items),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductionLogDetailDto> GetByIdAsync(int id)
    {
        var log = await _db.ProductionLogs
            .AsNoTracking()
            .Include(l => l.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(l => l.SupervisorUser)
            .Include(l => l.Photos).ThenInclude(p => p.UploadedByUser)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Production Log with ID {id} not found.");

        return _mapper.Map<ProductionLogDetailDto>(log);
    }

    public async Task<List<ProductionLogDto>> GetByPWOAsync(int pwoId)
    {
        var logs = await _db.ProductionLogs
            .AsNoTracking()
            .Include(l => l.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(l => l.SupervisorUser)
            .Include(l => l.Photos)
            .Where(l => l.ProductionWorkOrderId == pwoId)
            .OrderByDescending(l => l.Date)
            .ToListAsync();

        return _mapper.Map<List<ProductionLogDto>>(logs);
    }

    public async Task<ProductionLogDto> CreateAsync(CreateProductionLogDto dto, string userId)
    {
        // Validate PWO exists and is InProgress
        var pwo = await _db.ProductionWorkOrders.FindAsync(dto.ProductionWorkOrderId)
            ?? throw new ArgumentException("Production Work Order not found.");

        if (pwo.Status != "InProgress")
            throw new InvalidOperationException("Production Work Order must be InProgress to create a production log.");

        // Unique constraint: one log per PWO + date + shift
        var exists = await _db.ProductionLogs
            .AnyAsync(l => l.ProductionWorkOrderId == dto.ProductionWorkOrderId
                        && l.Date == dto.Date.Date
                        && l.Shift == dto.Shift);

        if (exists)
            throw new InvalidOperationException(
                $"A production log already exists for this PWO on {dto.Date:yyyy-MM-dd} ({dto.Shift} shift).");

        var log = _mapper.Map<ProductionLog>(dto);
        log.Date = dto.Date.Date;
        log.SupervisorUserId = userId;
        log.CreatedBy = userId;

        _db.ProductionLogs.Add(log);
        await _db.SaveChangesAsync();

        // Reload with navigation properties
        var created = await _db.ProductionLogs
            .AsNoTracking()
            .Include(l => l.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(l => l.SupervisorUser)
            .Include(l => l.Photos)
            .FirstAsync(l => l.Id == log.Id);

        return _mapper.Map<ProductionLogDto>(created);
    }

    public async Task<ProductionLogDto> UpdateAsync(int id, CreateProductionLogDto dto, string userId)
    {
        var log = await _db.ProductionLogs.FindAsync(id)
            ?? throw new KeyNotFoundException($"Production Log with ID {id} not found.");

        // Validate PWO
        var pwo = await _db.ProductionWorkOrders.FindAsync(dto.ProductionWorkOrderId)
            ?? throw new ArgumentException("Production Work Order not found.");

        if (pwo.Status != "InProgress")
            throw new InvalidOperationException("Production Work Order must be InProgress.");

        // Check unique constraint (exclude self)
        var exists = await _db.ProductionLogs
            .AnyAsync(l => l.ProductionWorkOrderId == dto.ProductionWorkOrderId
                        && l.Date == dto.Date.Date
                        && l.Shift == dto.Shift
                        && l.Id != id);

        if (exists)
            throw new InvalidOperationException(
                $"A production log already exists for this PWO on {dto.Date:yyyy-MM-dd} ({dto.Shift} shift).");

        log.Date = dto.Date.Date;
        log.Shift = dto.Shift;
        log.ProductionWorkOrderId = dto.ProductionWorkOrderId;
        log.ConveyorSpeedMtrPerMin = dto.ConveyorSpeedMtrPerMin;
        log.OvenTemperature = dto.OvenTemperature;
        log.PowderBatchNo = dto.PowderBatchNo;
        log.Remarks = dto.Remarks;
        log.UpdatedBy = userId;
        log.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        var updated = await _db.ProductionLogs
            .AsNoTracking()
            .Include(l => l.ProductionWorkOrder).ThenInclude(p => p.Customer)
            .Include(l => l.SupervisorUser)
            .Include(l => l.Photos)
            .FirstAsync(l => l.Id == id);

        return _mapper.Map<ProductionLogDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var log = await _db.ProductionLogs.FindAsync(id)
            ?? throw new KeyNotFoundException($"Production Log with ID {id} not found.");

        log.IsDeleted = true;
        log.UpdatedBy = userId;
        log.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<ProductionPhotoDto> UploadPhotoAsync(int id, IFormFile file, string? description, string userId)
    {
        var log = await _db.ProductionLogs
            .Include(l => l.Photos)
            .FirstOrDefaultAsync(l => l.Id == id)
            ?? throw new KeyNotFoundException($"Production Log with ID {id} not found.");

        // Validate photo count
        if (log.Photos.Count >= MaxPhotosPerLog)
            throw new InvalidOperationException($"Maximum {MaxPhotosPerLog} photos per production log.");

        // Validate file type
        if (!AllowedImageTypes.Contains(file.ContentType.ToLower()))
            throw new ArgumentException("Only image files (JPEG, PNG, WebP, GIF) are allowed.");

        // Validate file size
        if (file.Length > MaxFileSize)
            throw new ArgumentException("File size must not exceed 5 MB.");

        // Save file
        var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"),
            "uploads", "production-logs", id.ToString());
        Directory.CreateDirectory(uploadsDir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var photo = new ProductionPhoto
        {
            ProductionLogId = id,
            PhotoUrl = $"/uploads/production-logs/{id}/{fileName}",
            CapturedAt = DateTime.UtcNow,
            UploadedByUserId = userId,
            Description = description
        };

        _db.ProductionPhotos.Add(photo);
        await _db.SaveChangesAsync();

        // Reload with user nav
        var saved = await _db.ProductionPhotos
            .AsNoTracking()
            .Include(p => p.UploadedByUser)
            .FirstAsync(p => p.Id == photo.Id);

        return _mapper.Map<ProductionPhotoDto>(saved);
    }

    public async Task DeletePhotoAsync(int logId, int photoId, string userId)
    {
        var photo = await _db.ProductionPhotos
            .FirstOrDefaultAsync(p => p.Id == photoId && p.ProductionLogId == logId)
            ?? throw new KeyNotFoundException($"Photo with ID {photoId} not found for Production Log {logId}.");

        // Delete physical file
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var fullPath = Path.Combine(webRoot, photo.PhotoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        _db.ProductionPhotos.Remove(photo);
        await _db.SaveChangesAsync();
    }
}

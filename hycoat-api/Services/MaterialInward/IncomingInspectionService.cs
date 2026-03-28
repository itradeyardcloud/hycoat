using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.MaterialInward;
using HycoatApi.Models.Common;
using HycoatApi.Models.MaterialInward;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.MaterialInward;

public class IncomingInspectionService : IIncomingInspectionService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp", "image/heic"];
    private const long MaxPhotoSizeBytes = 5 * 1024 * 1024;

    public IncomingInspectionService(AppDbContext db, IMapper mapper, IWebHostEnvironment env)
    {
        _db = db;
        _mapper = mapper;
        _env = env;
    }

    public async Task<PagedResponse<IncomingInspectionDto>> GetAllAsync(
        string? search, string? overallStatus, int? materialInwardId,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.IncomingInspections
            .AsNoTracking()
            .Include(i => i.MaterialInward).ThenInclude(m => m.Customer)
            .Include(i => i.InspectedByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i =>
                i.InspectionNumber.ToLower().Contains(term) ||
                i.MaterialInward.Customer.Name.ToLower().Contains(term) ||
                i.MaterialInward.InwardNumber.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(overallStatus))
            query = query.Where(i => i.OverallStatus == overallStatus);

        if (materialInwardId.HasValue)
            query = query.Where(i => i.MaterialInwardId == materialInwardId.Value);

        if (fromDate.HasValue)
            query = query.Where(i => i.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(i => i.Date <= toDate.Value);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "inspectionnumber" => sortDesc ? query.OrderByDescending(i => i.InspectionNumber) : query.OrderBy(i => i.InspectionNumber),
            "customer" or "customername" => sortDesc ? query.OrderByDescending(i => i.MaterialInward.Customer.Name) : query.OrderBy(i => i.MaterialInward.Customer.Name),
            "overallstatus" or "status" => sortDesc ? query.OrderByDescending(i => i.OverallStatus) : query.OrderBy(i => i.OverallStatus),
            _ => sortDesc ? query.OrderByDescending(i => i.Date) : query.OrderBy(i => i.Date),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(i => new IncomingInspectionDto
            {
                Id = i.Id,
                InspectionNumber = i.InspectionNumber,
                Date = i.Date,
                MaterialInwardId = i.MaterialInwardId,
                InwardNumber = i.MaterialInward.InwardNumber,
                CustomerName = i.MaterialInward.Customer.Name,
                OverallStatus = i.OverallStatus,
                InspectedByName = i.InspectedByUser != null ? i.InspectedByUser.FullName : null,
            })
            .ToListAsync();

        return new PagedResponse<IncomingInspectionDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IncomingInspectionDetailDto> GetByIdAsync(int id)
    {
        var entity = await _db.IncomingInspections
            .AsNoTracking()
            .Include(i => i.MaterialInward).ThenInclude(m => m.Customer)
            .Include(i => i.InspectedByUser)
            .Include(i => i.Lines).ThenInclude(l => l.MaterialInwardLine).ThenInclude(ml => ml.SectionProfile)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Incoming Inspection with ID {id} not found.");

        var dto = new IncomingInspectionDetailDto
        {
            Id = entity.Id,
            InspectionNumber = entity.InspectionNumber,
            Date = entity.Date,
            MaterialInwardId = entity.MaterialInwardId,
            InwardNumber = entity.MaterialInward.InwardNumber,
            CustomerName = entity.MaterialInward.Customer.Name,
            OverallStatus = entity.OverallStatus,
            InspectedByName = entity.InspectedByUser?.FullName,
            Remarks = entity.Remarks,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Lines = entity.Lines.Select(l => new IncomingInspectionLineDetailDto
            {
                Id = l.Id,
                MaterialInwardLineId = l.MaterialInwardLineId,
                SectionNumber = l.MaterialInwardLine.SectionProfile.SectionNumber,
                QtyReceived = l.MaterialInwardLine.QtyReceived,
                WatermarkOk = l.WatermarkOk,
                ScratchOk = l.ScratchOk,
                DentOk = l.DentOk,
                DimensionalCheckOk = l.DimensionalCheckOk,
                BuffingRequired = l.BuffingRequired,
                BuffingCharge = l.BuffingCharge,
                Status = l.Status,
                Remarks = l.Remarks,
            }).ToList(),
        };

        // Load photos
        dto.Photos = await _db.FileAttachments
            .AsNoTracking()
            .Where(f => f.EntityType == "IncomingInspection" && f.EntityId == id && !f.IsDeleted)
            .Select(f => new FileAttachmentDto
            {
                Id = f.Id,
                FileName = f.FileName,
                StoredPath = f.StoredPath,
                ContentType = f.ContentType,
                FileSizeBytes = f.FileSizeBytes,
                UploadedAt = f.UploadedAt,
            })
            .ToListAsync();

        return dto;
    }

    public async Task<IncomingInspectionDto> CreateAsync(CreateIncomingInspectionDto dto, string userId)
    {
        // Validate material inward exists
        var inward = await _db.MaterialInwards
            .Include(m => m.Customer)
            .Include(m => m.Lines)
            .FirstOrDefaultAsync(m => m.Id == dto.MaterialInwardId)
            ?? throw new ArgumentException("Material Inward not found.");

        // Validate uniqueness: one inspection per inward
        var alreadyExists = await _db.IncomingInspections
            .AnyAsync(i => i.MaterialInwardId == dto.MaterialInwardId);
        if (alreadyExists)
            throw new ArgumentException("An inspection already exists for this Material Inward.");

        // Validate that line references are valid
        var inwardLineIds = inward.Lines.Select(l => l.Id).ToHashSet();
        foreach (var line in dto.Lines)
        {
            if (!inwardLineIds.Contains(line.MaterialInwardLineId))
                throw new ArgumentException($"Material Inward Line ID {line.MaterialInwardLineId} does not belong to this inward.");
        }

        // Derive overall status
        var overallStatus = DeriveOverallStatus(dto.Lines.Select(l => l.Status));

        var entity = new IncomingInspection
        {
            InspectionNumber = await GenerateInspectionNumberAsync(),
            Date = dto.Date,
            MaterialInwardId = dto.MaterialInwardId,
            InspectedByUserId = userId,
            OverallStatus = overallStatus,
            Remarks = dto.Remarks,
            CreatedBy = userId,
        };

        foreach (var lineDto in dto.Lines)
        {
            entity.Lines.Add(new IncomingInspectionLine
            {
                MaterialInwardLineId = lineDto.MaterialInwardLineId,
                WatermarkOk = lineDto.WatermarkOk,
                ScratchOk = lineDto.ScratchOk,
                DentOk = lineDto.DentOk,
                DimensionalCheckOk = lineDto.DimensionalCheckOk,
                BuffingRequired = lineDto.BuffingRequired,
                BuffingCharge = lineDto.BuffingRequired ? lineDto.BuffingCharge : null,
                Status = lineDto.Status,
                Remarks = lineDto.Remarks,
            });
        }

        _db.IncomingInspections.Add(entity);

        // Update MaterialInward status to Inspected
        inward.Status = "Inspected";
        inward.UpdatedBy = userId;
        inward.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return new IncomingInspectionDto
        {
            Id = entity.Id,
            InspectionNumber = entity.InspectionNumber,
            Date = entity.Date,
            MaterialInwardId = entity.MaterialInwardId,
            InwardNumber = inward.InwardNumber,
            CustomerName = inward.Customer.Name,
            OverallStatus = entity.OverallStatus,
        };
    }

    public async Task<IncomingInspectionDto> UpdateAsync(int id, CreateIncomingInspectionDto dto, string userId)
    {
        var entity = await _db.IncomingInspections
            .Include(i => i.Lines)
            .Include(i => i.MaterialInward).ThenInclude(m => m.Customer)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Incoming Inspection with ID {id} not found.");

        // Validate line references
        var inwardLineIds = await _db.MaterialInwardLines
            .Where(l => l.MaterialInwardId == entity.MaterialInwardId)
            .Select(l => l.Id)
            .ToHashSetAsync();

        foreach (var line in dto.Lines)
        {
            if (!inwardLineIds.Contains(line.MaterialInwardLineId))
                throw new ArgumentException($"Material Inward Line ID {line.MaterialInwardLineId} does not belong to this inward.");
        }

        // Derive overall status
        var overallStatus = DeriveOverallStatus(dto.Lines.Select(l => l.Status));

        entity.Date = dto.Date;
        entity.OverallStatus = overallStatus;
        entity.Remarks = dto.Remarks;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        // Replace all lines
        _db.IncomingInspectionLines.RemoveRange(entity.Lines);
        foreach (var lineDto in dto.Lines)
        {
            entity.Lines.Add(new IncomingInspectionLine
            {
                MaterialInwardLineId = lineDto.MaterialInwardLineId,
                WatermarkOk = lineDto.WatermarkOk,
                ScratchOk = lineDto.ScratchOk,
                DentOk = lineDto.DentOk,
                DimensionalCheckOk = lineDto.DimensionalCheckOk,
                BuffingRequired = lineDto.BuffingRequired,
                BuffingCharge = lineDto.BuffingRequired ? lineDto.BuffingCharge : null,
                Status = lineDto.Status,
                Remarks = lineDto.Remarks,
            });
        }

        await _db.SaveChangesAsync();

        return new IncomingInspectionDto
        {
            Id = entity.Id,
            InspectionNumber = entity.InspectionNumber,
            Date = entity.Date,
            MaterialInwardId = entity.MaterialInwardId,
            InwardNumber = entity.MaterialInward.InwardNumber,
            CustomerName = entity.MaterialInward.Customer.Name,
            OverallStatus = entity.OverallStatus,
        };
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var entity = await _db.IncomingInspections.FindAsync(id)
            ?? throw new KeyNotFoundException($"Incoming Inspection with ID {id} not found.");

        entity.IsDeleted = true;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<List<FileAttachmentDto>> UploadPhotosAsync(int id, List<IFormFile> files, string userId)
    {
        var entity = await _db.IncomingInspections.FindAsync(id)
            ?? throw new KeyNotFoundException($"Incoming Inspection with ID {id} not found.");

        var uploadsDir = Path.Combine(
            _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"),
            "uploads", "incoming-inspection", id.ToString());
        Directory.CreateDirectory(uploadsDir);

        var result = new List<FileAttachmentDto>();

        foreach (var file in files)
        {
            if (file.Length > MaxPhotoSizeBytes)
                throw new ArgumentException($"File '{file.FileName}' exceeds maximum size of 5MB.");

            if (!AllowedImageTypes.Contains(file.ContentType?.ToLower()))
                throw new ArgumentException($"File '{file.FileName}' has unsupported type. Allowed: JPEG, PNG, WebP, HEIC.");

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var storedPath = $"/uploads/incoming-inspection/{id}/{fileName}";
            var attachment = new FileAttachment
            {
                FileName = file.FileName,
                StoredPath = storedPath,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                EntityType = "IncomingInspection",
                EntityId = id,
                UploadedBy = userId,
                UploadedAt = DateTime.UtcNow,
                CreatedBy = userId,
            };

            _db.FileAttachments.Add(attachment);
            await _db.SaveChangesAsync();

            result.Add(new FileAttachmentDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                StoredPath = attachment.StoredPath,
                ContentType = attachment.ContentType,
                FileSizeBytes = attachment.FileSizeBytes,
                UploadedAt = attachment.UploadedAt,
            });
        }

        return result;
    }

    public async Task<List<FileAttachmentDto>> GetPhotosAsync(int id)
    {
        var exists = await _db.IncomingInspections.AnyAsync(i => i.Id == id);
        if (!exists)
            throw new KeyNotFoundException($"Incoming Inspection with ID {id} not found.");

        return await _db.FileAttachments
            .AsNoTracking()
            .Where(f => f.EntityType == "IncomingInspection" && f.EntityId == id && !f.IsDeleted)
            .Select(f => new FileAttachmentDto
            {
                Id = f.Id,
                FileName = f.FileName,
                StoredPath = f.StoredPath,
                ContentType = f.ContentType,
                FileSizeBytes = f.FileSizeBytes,
                UploadedAt = f.UploadedAt,
            })
            .ToListAsync();
    }

    private static string DeriveOverallStatus(IEnumerable<string> lineStatuses)
    {
        var statuses = lineStatuses.ToList();
        if (statuses.Any(s => s == "Fail")) return "Fail";
        if (statuses.Any(s => s == "Conditional")) return "Conditional";
        return "Pass";
    }

    private async Task<string> GenerateInspectionNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"IIR-{year}-";

        var lastNumber = await _db.IncomingInspections
            .IgnoreQueryFilters()
            .Where(i => i.InspectionNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InspectionNumber)
            .Select(i => i.InspectionNumber)
            .FirstOrDefaultAsync();

        var nextSeq = 1;
        if (lastNumber != null)
        {
            var parts = lastNumber.Split('-');
            if (parts.Length == 3 && int.TryParse(parts[2], out var lastSeq))
                nextSeq = lastSeq + 1;
        }

        return $"{prefix}{nextSeq:D3}";
    }
}

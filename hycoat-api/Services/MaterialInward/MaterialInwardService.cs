using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.MaterialInward;
using HycoatApi.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.MaterialInward;

public class MaterialInwardService : IMaterialInwardService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;

    private static readonly Dictionary<string, string[]> StatusTransitions = new()
    {
        ["Received"] = ["InspectionPending"],
        ["InspectionPending"] = ["Inspected"],
        ["Inspected"] = ["Stored"],
    };

    private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/webp", "image/heic"];
    private const int MaxPhotosPerInward = 10;
    private const long MaxPhotoSizeBytes = 5 * 1024 * 1024; // 5MB

    public MaterialInwardService(AppDbContext db, IMapper mapper, IWebHostEnvironment env)
    {
        _db = db;
        _mapper = mapper;
        _env = env;
    }

    public async Task<PagedResponse<MaterialInwardDto>> GetAllAsync(
        string? search, string? status, int? customerId, int? workOrderId,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.MaterialInwards
            .AsNoTracking()
            .Include(m => m.Customer)
            .Include(m => m.WorkOrder)
            .Include(m => m.Lines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(m =>
                m.InwardNumber.ToLower().Contains(term) ||
                m.Customer.Name.ToLower().Contains(term) ||
                (m.CustomerDCNumber != null && m.CustomerDCNumber.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(m => m.Status == status);

        if (customerId.HasValue)
            query = query.Where(m => m.CustomerId == customerId.Value);

        if (workOrderId.HasValue)
            query = query.Where(m => m.WorkOrderId == workOrderId.Value);

        if (fromDate.HasValue)
            query = query.Where(m => m.Date >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(m => m.Date <= toDate.Value);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "inwardnumber" => sortDesc ? query.OrderByDescending(m => m.InwardNumber) : query.OrderBy(m => m.InwardNumber),
            "customer" or "customername" => sortDesc ? query.OrderByDescending(m => m.Customer.Name) : query.OrderBy(m => m.Customer.Name),
            "status" => sortDesc ? query.OrderByDescending(m => m.Status) : query.OrderBy(m => m.Status),
            _ => sortDesc ? query.OrderByDescending(m => m.Date) : query.OrderBy(m => m.Date),
        };

        // Get photo counts for these inwards
        var inwardIds = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => m.Id)
            .ToListAsync();

        var photoCountMap = await _db.FileAttachments
            .AsNoTracking()
            .Where(f => f.EntityType == "MaterialInward" && inwardIds.Contains(f.EntityId) && !f.IsDeleted)
            .GroupBy(f => f.EntityId)
            .Select(g => new { EntityId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.EntityId, x => x.Count);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MaterialInwardDto
            {
                Id = m.Id,
                InwardNumber = m.InwardNumber,
                Date = m.Date,
                CustomerName = m.Customer.Name,
                CustomerId = m.CustomerId,
                WorkOrderId = m.WorkOrderId,
                WONumber = m.WorkOrder != null ? m.WorkOrder.WONumber : null,
                VehicleNumber = m.VehicleNumber,
                CustomerDCNumber = m.CustomerDCNumber,
                Status = m.Status,
                LineCount = m.Lines.Count,
            })
            .ToListAsync();

        // Set HasPhotos from the photo count map
        foreach (var item in items)
        {
            item.HasPhotos = photoCountMap.ContainsKey(item.Id);
        }

        return new PagedResponse<MaterialInwardDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<MaterialInwardDetailDto> GetByIdAsync(int id)
    {
        var entity = await _db.MaterialInwards
            .AsNoTracking()
            .Include(m => m.Customer)
            .Include(m => m.WorkOrder)
            .Include(m => m.ProcessType)
            .Include(m => m.PowderColor)
            .Include(m => m.ReceivedByUser)
            .Include(m => m.Lines).ThenInclude(l => l.SectionProfile)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Material Inward with ID {id} not found.");

        var dto = new MaterialInwardDetailDto
        {
            Id = entity.Id,
            InwardNumber = entity.InwardNumber,
            Date = entity.Date,
            CustomerName = entity.Customer.Name,
            CustomerId = entity.CustomerId,
            WorkOrderId = entity.WorkOrderId,
            WONumber = entity.WorkOrder?.WONumber,
            VehicleNumber = entity.VehicleNumber,
            CustomerDCNumber = entity.CustomerDCNumber,
            CustomerDCDate = entity.CustomerDCDate,
            UnloadingLocation = entity.UnloadingLocation,
            ProcessTypeId = entity.ProcessTypeId,
            ProcessTypeName = entity.ProcessType?.Name,
            PowderColorId = entity.PowderColorId,
            PowderColorName = entity.PowderColor?.ColorName,
            ReceivedByName = entity.ReceivedByUser?.FullName,
            Status = entity.Status,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            LineCount = entity.Lines.Count,
            Lines = entity.Lines.Select(l => new MaterialInwardLineDto
            {
                Id = l.Id,
                SectionProfileId = l.SectionProfileId,
                SectionNumber = l.SectionProfile.SectionNumber,
                LengthMM = l.LengthMM,
                QtyAsPerDC = l.QtyAsPerDC,
                QtyReceived = l.QtyReceived,
                WeightKg = l.WeightKg,
                Discrepancy = l.Discrepancy,
                Remarks = l.Remarks,
            }).ToList(),
        };

        // Load photos
        dto.Photos = await _db.FileAttachments
            .AsNoTracking()
            .Where(f => f.EntityType == "MaterialInward" && f.EntityId == id && !f.IsDeleted)
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

        dto.HasPhotos = dto.Photos.Count > 0;

        return dto;
    }

    public async Task<MaterialInwardDto> CreateAsync(CreateMaterialInwardDto dto, string userId)
    {
        // Validate customer exists
        var customerExists = await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId);
        if (!customerExists)
            throw new ArgumentException("Customer not found.");

        // Validate work order if provided
        if (dto.WorkOrderId.HasValue)
        {
            var woExists = await _db.WorkOrders.AnyAsync(wo => wo.Id == dto.WorkOrderId.Value);
            if (!woExists)
                throw new ArgumentException("Work Order not found.");
        }

        // Validate process type if provided
        if (dto.ProcessTypeId.HasValue)
        {
            var ptExists = await _db.ProcessTypes.AnyAsync(pt => pt.Id == dto.ProcessTypeId.Value);
            if (!ptExists)
                throw new ArgumentException("Process type not found.");
        }

        // Validate powder color if provided
        if (dto.PowderColorId.HasValue)
        {
            var pcExists = await _db.PowderColors.AnyAsync(pc => pc.Id == dto.PowderColorId.Value);
            if (!pcExists)
                throw new ArgumentException("Powder color not found.");
        }

        // Validate section profiles exist
        var sectionIds = dto.Lines.Select(l => l.SectionProfileId).Distinct().ToList();
        var existingSectionCount = await _db.SectionProfiles.CountAsync(s => sectionIds.Contains(s.Id));
        if (existingSectionCount != sectionIds.Count)
            throw new ArgumentException("One or more section profiles not found.");

        var entity = new Models.MaterialInward.MaterialInward
        {
            InwardNumber = await GenerateInwardNumberAsync(),
            Date = dto.Date,
            CustomerId = dto.CustomerId,
            WorkOrderId = dto.WorkOrderId,
            CustomerDCNumber = dto.CustomerDCNumber,
            CustomerDCDate = dto.CustomerDCDate,
            VehicleNumber = dto.VehicleNumber,
            UnloadingLocation = dto.UnloadingLocation,
            ProcessTypeId = dto.ProcessTypeId,
            PowderColorId = dto.PowderColorId,
            ReceivedByUserId = userId,
            Status = "Received",
            Notes = dto.Notes,
            CreatedBy = userId,
        };

        foreach (var lineDto in dto.Lines)
        {
            entity.Lines.Add(new Models.MaterialInward.MaterialInwardLine
            {
                SectionProfileId = lineDto.SectionProfileId,
                LengthMM = lineDto.LengthMM,
                QtyAsPerDC = lineDto.QtyAsPerDC,
                QtyReceived = lineDto.QtyReceived,
                WeightKg = lineDto.WeightKg,
                Discrepancy = lineDto.QtyReceived - lineDto.QtyAsPerDC,
                Remarks = lineDto.Remarks,
            });
        }

        _db.MaterialInwards.Add(entity);

        // Update WO status to MaterialReceived if linked
        if (dto.WorkOrderId.HasValue)
        {
            var wo = await _db.WorkOrders.FindAsync(dto.WorkOrderId.Value);
            if (wo != null && (wo.Status == "Created" || wo.Status == "MaterialAwaited"))
            {
                wo.Status = "MaterialReceived";
                wo.UpdatedBy = userId;
                wo.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _db.SaveChangesAsync();

        // Reload for response
        var created = await _db.MaterialInwards
            .AsNoTracking()
            .Include(m => m.Customer)
            .Include(m => m.WorkOrder)
            .Include(m => m.Lines)
            .FirstAsync(m => m.Id == entity.Id);

        return new MaterialInwardDto
        {
            Id = created.Id,
            InwardNumber = created.InwardNumber,
            Date = created.Date,
            CustomerName = created.Customer.Name,
            CustomerId = created.CustomerId,
            WorkOrderId = created.WorkOrderId,
            WONumber = created.WorkOrder?.WONumber,
            VehicleNumber = created.VehicleNumber,
            CustomerDCNumber = created.CustomerDCNumber,
            Status = created.Status,
            LineCount = created.Lines.Count,
        };
    }

    public async Task<MaterialInwardDto> UpdateAsync(int id, UpdateMaterialInwardDto dto, string userId)
    {
        var entity = await _db.MaterialInwards
            .Include(m => m.Lines)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Material Inward with ID {id} not found.");

        // Validate customer exists
        var customerExists = await _db.Customers.AnyAsync(c => c.Id == dto.CustomerId);
        if (!customerExists)
            throw new ArgumentException("Customer not found.");

        // Validate work order if provided
        if (dto.WorkOrderId.HasValue)
        {
            var woExists = await _db.WorkOrders.AnyAsync(wo => wo.Id == dto.WorkOrderId.Value);
            if (!woExists)
                throw new ArgumentException("Work Order not found.");
        }

        // Validate section profiles
        var sectionIds = dto.Lines.Select(l => l.SectionProfileId).Distinct().ToList();
        var existingSectionCount = await _db.SectionProfiles.CountAsync(s => sectionIds.Contains(s.Id));
        if (existingSectionCount != sectionIds.Count)
            throw new ArgumentException("One or more section profiles not found.");

        // Update header
        entity.Date = dto.Date;
        entity.CustomerId = dto.CustomerId;
        entity.WorkOrderId = dto.WorkOrderId;
        entity.CustomerDCNumber = dto.CustomerDCNumber;
        entity.CustomerDCDate = dto.CustomerDCDate;
        entity.VehicleNumber = dto.VehicleNumber;
        entity.UnloadingLocation = dto.UnloadingLocation;
        entity.ProcessTypeId = dto.ProcessTypeId;
        entity.PowderColorId = dto.PowderColorId;
        entity.Notes = dto.Notes;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        // Diff lines: update existing, add new, remove missing
        var incomingLineIds = dto.Lines.Where(l => l.Id.HasValue).Select(l => l.Id!.Value).ToHashSet();
        var linesToRemove = entity.Lines.Where(l => !incomingLineIds.Contains(l.Id)).ToList();
        foreach (var remove in linesToRemove)
            _db.MaterialInwardLines.Remove(remove);

        foreach (var lineDto in dto.Lines)
        {
            if (lineDto.Id.HasValue)
            {
                var existing = entity.Lines.FirstOrDefault(l => l.Id == lineDto.Id.Value);
                if (existing != null)
                {
                    existing.SectionProfileId = lineDto.SectionProfileId;
                    existing.LengthMM = lineDto.LengthMM;
                    existing.QtyAsPerDC = lineDto.QtyAsPerDC;
                    existing.QtyReceived = lineDto.QtyReceived;
                    existing.WeightKg = lineDto.WeightKg;
                    existing.Discrepancy = lineDto.QtyReceived - lineDto.QtyAsPerDC;
                    existing.Remarks = lineDto.Remarks;
                }
            }
            else
            {
                entity.Lines.Add(new Models.MaterialInward.MaterialInwardLine
                {
                    SectionProfileId = lineDto.SectionProfileId,
                    LengthMM = lineDto.LengthMM,
                    QtyAsPerDC = lineDto.QtyAsPerDC,
                    QtyReceived = lineDto.QtyReceived,
                    WeightKg = lineDto.WeightKg,
                    Discrepancy = lineDto.QtyReceived - lineDto.QtyAsPerDC,
                    Remarks = lineDto.Remarks,
                });
            }
        }

        await _db.SaveChangesAsync();

        // Reload for response
        var updated = await _db.MaterialInwards
            .AsNoTracking()
            .Include(m => m.Customer)
            .Include(m => m.WorkOrder)
            .Include(m => m.Lines)
            .FirstAsync(m => m.Id == id);

        return new MaterialInwardDto
        {
            Id = updated.Id,
            InwardNumber = updated.InwardNumber,
            Date = updated.Date,
            CustomerName = updated.Customer.Name,
            CustomerId = updated.CustomerId,
            WorkOrderId = updated.WorkOrderId,
            WONumber = updated.WorkOrder?.WONumber,
            VehicleNumber = updated.VehicleNumber,
            CustomerDCNumber = updated.CustomerDCNumber,
            Status = updated.Status,
            LineCount = updated.Lines.Count,
        };
    }

    public async Task UpdateStatusAsync(int id, UpdateMaterialInwardStatusDto dto, string userId)
    {
        var entity = await _db.MaterialInwards.FindAsync(id)
            ?? throw new KeyNotFoundException($"Material Inward with ID {id} not found.");

        if (!StatusTransitions.TryGetValue(entity.Status, out var allowed) || !allowed.Contains(dto.Status))
            throw new InvalidOperationException(
                $"Cannot transition from '{entity.Status}' to '{dto.Status}'.");

        entity.Status = dto.Status;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var entity = await _db.MaterialInwards.FindAsync(id)
            ?? throw new KeyNotFoundException($"Material Inward with ID {id} not found.");

        entity.IsDeleted = true;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<List<LookupDto>> GetLookupAsync(bool? hasInspection = null)
    {
        var query = _db.MaterialInwards
            .AsNoTracking()
            .Include(m => m.IncomingInspections)
            .AsQueryable();

        if (hasInspection == false)
            query = query.Where(m => !m.IncomingInspections.Any());
        else if (hasInspection == true)
            query = query.Where(m => m.IncomingInspections.Any());

        return await query
            .OrderByDescending(m => m.Date)
            .Select(m => new LookupDto { Id = m.Id, Name = m.InwardNumber })
            .ToListAsync();
    }

    public async Task<List<FileAttachmentDto>> UploadPhotosAsync(int id, List<IFormFile> files, string userId)
    {
        var entity = await _db.MaterialInwards.FindAsync(id)
            ?? throw new KeyNotFoundException($"Material Inward with ID {id} not found.");

        // Check existing photo count
        var existingCount = await _db.FileAttachments
            .CountAsync(f => f.EntityType == "MaterialInward" && f.EntityId == id && !f.IsDeleted);

        if (existingCount + files.Count > MaxPhotosPerInward)
            throw new ArgumentException($"Maximum {MaxPhotosPerInward} photos allowed per inward. Currently {existingCount} uploaded.");

        var uploadsDir = Path.Combine(
            _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"),
            "uploads", "material-inward", id.ToString());
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

            var storedPath = $"/uploads/material-inward/{id}/{fileName}";
            var attachment = new FileAttachment
            {
                FileName = file.FileName,
                StoredPath = storedPath,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                EntityType = "MaterialInward",
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
        var exists = await _db.MaterialInwards.AnyAsync(m => m.Id == id);
        if (!exists)
            throw new KeyNotFoundException($"Material Inward with ID {id} not found.");

        return await _db.FileAttachments
            .AsNoTracking()
            .Where(f => f.EntityType == "MaterialInward" && f.EntityId == id && !f.IsDeleted)
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

    public async Task DeletePhotoAsync(int id, int photoId, string userId)
    {
        var photo = await _db.FileAttachments
            .FirstOrDefaultAsync(f => f.Id == photoId && f.EntityType == "MaterialInward" && f.EntityId == id && !f.IsDeleted)
            ?? throw new KeyNotFoundException("Photo not found.");

        photo.IsDeleted = true;
        photo.UpdatedBy = userId;
        photo.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    private async Task<string> GenerateInwardNumberAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"INW-{year}-";

        var lastNumber = await _db.MaterialInwards
            .IgnoreQueryFilters()
            .Where(m => m.InwardNumber.StartsWith(prefix))
            .OrderByDescending(m => m.InwardNumber)
            .Select(m => m.InwardNumber)
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

    public async Task<List<WorkOrderLookupDto>> GetWorkOrderLookupAsync(string? search)
    {
        var query = _db.WorkOrders
            .Include(w => w.Customer)
            .Include(w => w.ProcessType)
            .Include(w => w.PowderColor)
            .Where(w => !w.IsDeleted && w.Status != "Closed" && w.Status != "Cancelled")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(w =>
                w.WONumber.ToLower().Contains(term) ||
                w.Customer.Name.ToLower().Contains(term));
        }

        return await query
            .OrderByDescending(w => w.Date)
            .Take(50)
            .Select(w => new WorkOrderLookupDto
            {
                Id = w.Id,
                WONumber = w.WONumber,
                CustomerId = w.CustomerId,
                CustomerName = w.Customer.Name,
                ProcessTypeId = w.ProcessTypeId,
                ProcessTypeName = w.ProcessType.Name,
                PowderColorId = w.PowderColorId,
                PowderColorName = w.PowderColor != null ? w.PowderColor.ColorName : null,
            })
            .ToListAsync();
    }
}

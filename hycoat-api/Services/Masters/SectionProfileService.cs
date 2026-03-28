using AutoMapper;
using AutoMapper.QueryableExtensions;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Masters;
using HycoatApi.Models.Masters;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Masters;

public class SectionProfileService : ISectionProfileService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;
    private readonly IWebHostEnvironment _env;

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".pdf"];
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB

    public SectionProfileService(AppDbContext db, IMapper mapper, IWebHostEnvironment env)
    {
        _db = db;
        _mapper = mapper;
        _env = env;
    }

    public async Task<PagedResponse<SectionProfileDto>> GetAllAsync(string? search, int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.SectionProfiles.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s =>
                s.SectionNumber.ToLower().Contains(term) ||
                (s.Type != null && s.Type.ToLower().Contains(term)));
        }

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "type" => sortDesc ? query.OrderByDescending(s => s.Type) : query.OrderBy(s => s.Type),
            "perimetermm" => sortDesc ? query.OrderByDescending(s => s.PerimeterMM) : query.OrderBy(s => s.PerimeterMM),
            "createdat" => sortDesc ? query.OrderByDescending(s => s.CreatedAt) : query.OrderBy(s => s.CreatedAt),
            _ => sortDesc ? query.OrderByDescending(s => s.SectionNumber) : query.OrderBy(s => s.SectionNumber),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<SectionProfileDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResponse<SectionProfileDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<SectionProfileDetailDto> GetByIdAsync(int id)
    {
        var entity = await _db.SectionProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException($"Section profile with ID {id} not found.");

        return _mapper.Map<SectionProfileDetailDto>(entity);
    }

    public async Task<SectionProfileDto> CreateAsync(CreateSectionProfileDto dto, string userId)
    {
        var entity = _mapper.Map<SectionProfile>(dto);
        entity.CreatedBy = userId;

        _db.SectionProfiles.Add(entity);
        await _db.SaveChangesAsync();

        return _mapper.Map<SectionProfileDto>(entity);
    }

    public async Task<SectionProfileDto> UpdateAsync(int id, UpdateSectionProfileDto dto, string userId)
    {
        var entity = await _db.SectionProfiles.FindAsync(id)
            ?? throw new KeyNotFoundException($"Section profile with ID {id} not found.");

        _mapper.Map(dto, entity);
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<SectionProfileDto>(entity);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var entity = await _db.SectionProfiles.FindAsync(id)
            ?? throw new KeyNotFoundException($"Section profile with ID {id} not found.");

        entity.IsDeleted = true;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<List<SectionProfileLookupDto>> GetLookupAsync()
    {
        return await _db.SectionProfiles
            .AsNoTracking()
            .OrderBy(s => s.SectionNumber)
            .ProjectTo<SectionProfileLookupDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<string> UploadDrawingAsync(int id, IFormFile file, string userId)
    {
        var entity = await _db.SectionProfiles.FindAsync(id)
            ?? throw new KeyNotFoundException($"Section profile with ID {id} not found.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(ext))
            throw new InvalidOperationException("Only .jpg, .jpeg, .png, and .pdf files are allowed.");

        if (file.Length > MaxFileSize)
            throw new InvalidOperationException("File size must not exceed 10MB.");

        var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "uploads", "drawings");
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{id}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var url = $"/uploads/drawings/{fileName}";
        entity.DrawingFileUrl = url;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return url;
    }
}

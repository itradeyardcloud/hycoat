using AutoMapper;
using AutoMapper.QueryableExtensions;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Masters;
using HycoatApi.Models.Masters;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Masters;

public class PowderColorService : IPowderColorService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public PowderColorService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResponse<PowderColorDto>> GetAllAsync(string? search, int? vendorId, int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.PowderColors.AsNoTracking().Include(p => p.Vendor).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.PowderCode.ToLower().Contains(term) ||
                p.ColorName.ToLower().Contains(term) ||
                (p.RALCode != null && p.RALCode.ToLower().Contains(term)) ||
                (p.Make != null && p.Make.ToLower().Contains(term)));
        }

        if (vendorId.HasValue)
            query = query.Where(p => p.VendorId == vendorId.Value);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "colorname" => sortDesc ? query.OrderByDescending(p => p.ColorName) : query.OrderBy(p => p.ColorName),
            "ralcode" => sortDesc ? query.OrderByDescending(p => p.RALCode) : query.OrderBy(p => p.RALCode),
            "createdat" => sortDesc ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => sortDesc ? query.OrderByDescending(p => p.PowderCode) : query.OrderBy(p => p.PowderCode),
        };

        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = _mapper.Map<List<PowderColorDto>>(entities);

        return new PagedResponse<PowderColorDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PowderColorDetailDto> GetByIdAsync(int id)
    {
        var entity = await _db.PowderColors
            .AsNoTracking()
            .Include(p => p.Vendor)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Powder color with ID {id} not found.");

        return _mapper.Map<PowderColorDetailDto>(entity);
    }

    public async Task<PowderColorDto> CreateAsync(CreatePowderColorDto dto, string userId)
    {
        var entity = _mapper.Map<PowderColor>(dto);
        entity.CreatedBy = userId;

        _db.PowderColors.Add(entity);
        await _db.SaveChangesAsync();

        // Reload with vendor for response mapping
        await _db.Entry(entity).Reference(p => p.Vendor).LoadAsync();

        return _mapper.Map<PowderColorDto>(entity);
    }

    public async Task<PowderColorDto> UpdateAsync(int id, UpdatePowderColorDto dto, string userId)
    {
        var entity = await _db.PowderColors.FindAsync(id)
            ?? throw new KeyNotFoundException($"Powder color with ID {id} not found.");

        _mapper.Map(dto, entity);
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _db.Entry(entity).Reference(p => p.Vendor).LoadAsync();

        return _mapper.Map<PowderColorDto>(entity);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var entity = await _db.PowderColors.FindAsync(id)
            ?? throw new KeyNotFoundException($"Powder color with ID {id} not found.");

        entity.IsDeleted = true;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<List<PowderColorLookupDto>> GetLookupAsync()
    {
        return await _db.PowderColors
            .AsNoTracking()
            .OrderBy(p => p.PowderCode)
            .ProjectTo<PowderColorLookupDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}

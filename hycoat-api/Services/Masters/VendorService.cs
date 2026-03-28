using AutoMapper;
using AutoMapper.QueryableExtensions;
using HycoatApi.Data;
using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Masters;
using HycoatApi.Models.Masters;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Masters;

public class VendorService : IVendorService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public VendorService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<PagedResponse<VendorDto>> GetAllAsync(string? search, string? vendorType, int page, int pageSize, string sortBy, bool sortDesc)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = _db.Vendors.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(v =>
                v.Name.ToLower().Contains(term) ||
                (v.City != null && v.City.ToLower().Contains(term)) ||
                (v.GSTIN != null && v.GSTIN.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(vendorType))
            query = query.Where(v => v.VendorType == vendorType);

        var totalCount = await query.CountAsync();

        query = sortBy.ToLower() switch
        {
            "vendortype" => sortDesc ? query.OrderByDescending(v => v.VendorType) : query.OrderBy(v => v.VendorType),
            "city" => sortDesc ? query.OrderByDescending(v => v.City) : query.OrderBy(v => v.City),
            "createdat" => sortDesc ? query.OrderByDescending(v => v.CreatedAt) : query.OrderBy(v => v.CreatedAt),
            _ => sortDesc ? query.OrderByDescending(v => v.Name) : query.OrderBy(v => v.Name),
        };

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ProjectTo<VendorDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new PagedResponse<VendorDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<VendorDetailDto> GetByIdAsync(int id)
    {
        var entity = await _db.Vendors
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException($"Vendor with ID {id} not found.");

        return _mapper.Map<VendorDetailDto>(entity);
    }

    public async Task<VendorDto> CreateAsync(CreateVendorDto dto, string userId)
    {
        var entity = _mapper.Map<Vendor>(dto);
        entity.CreatedBy = userId;

        _db.Vendors.Add(entity);
        await _db.SaveChangesAsync();

        return _mapper.Map<VendorDto>(entity);
    }

    public async Task<VendorDto> UpdateAsync(int id, UpdateVendorDto dto, string userId)
    {
        var entity = await _db.Vendors.FindAsync(id)
            ?? throw new KeyNotFoundException($"Vendor with ID {id} not found.");

        _mapper.Map(dto, entity);
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<VendorDto>(entity);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var entity = await _db.Vendors.FindAsync(id)
            ?? throw new KeyNotFoundException($"Vendor with ID {id} not found.");

        entity.IsDeleted = true;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<List<LookupDto>> GetLookupAsync()
    {
        return await _db.Vendors
            .AsNoTracking()
            .OrderBy(v => v.Name)
            .ProjectTo<LookupDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }
}

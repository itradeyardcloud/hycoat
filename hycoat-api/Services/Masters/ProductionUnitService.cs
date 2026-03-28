using AutoMapper;
using AutoMapper.QueryableExtensions;
using HycoatApi.Data;
using HycoatApi.DTOs.Masters;
using HycoatApi.Models.Masters;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Masters;

public class ProductionUnitService : IProductionUnitService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public ProductionUnitService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<ProductionUnitDto>> GetAllAsync()
    {
        return await _db.ProductionUnits
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ProjectTo<ProductionUnitDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ProductionUnitDto> GetByIdAsync(int id)
    {
        var entity = await _db.ProductionUnits
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Production unit with ID {id} not found.");

        return _mapper.Map<ProductionUnitDto>(entity);
    }

    public async Task<ProductionUnitDto> CreateAsync(CreateProductionUnitDto dto, string userId)
    {
        var entity = _mapper.Map<ProductionUnit>(dto);
        entity.CreatedBy = userId;

        _db.ProductionUnits.Add(entity);
        await _db.SaveChangesAsync();

        return _mapper.Map<ProductionUnitDto>(entity);
    }

    public async Task<ProductionUnitDto> UpdateAsync(int id, UpdateProductionUnitDto dto, string userId)
    {
        var entity = await _db.ProductionUnits.FindAsync(id)
            ?? throw new KeyNotFoundException($"Production unit with ID {id} not found.");

        _mapper.Map(dto, entity);
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<ProductionUnitDto>(entity);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var entity = await _db.ProductionUnits.FindAsync(id)
            ?? throw new KeyNotFoundException($"Production unit with ID {id} not found.");

        entity.IsDeleted = true;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}

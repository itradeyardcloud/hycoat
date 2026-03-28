using AutoMapper;
using AutoMapper.QueryableExtensions;
using HycoatApi.Data;
using HycoatApi.DTOs.Masters;
using HycoatApi.Models.Masters;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Masters;

public class ProcessTypeService : IProcessTypeService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public ProcessTypeService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<ProcessTypeDto>> GetAllAsync()
    {
        return await _db.ProcessTypes
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ProjectTo<ProcessTypeDto>(_mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<ProcessTypeDto> GetByIdAsync(int id)
    {
        var entity = await _db.ProcessTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Process type with ID {id} not found.");

        return _mapper.Map<ProcessTypeDto>(entity);
    }

    public async Task<ProcessTypeDto> CreateAsync(CreateProcessTypeDto dto, string userId)
    {
        var entity = _mapper.Map<ProcessType>(dto);
        entity.CreatedBy = userId;

        _db.ProcessTypes.Add(entity);
        await _db.SaveChangesAsync();

        return _mapper.Map<ProcessTypeDto>(entity);
    }

    public async Task<ProcessTypeDto> UpdateAsync(int id, UpdateProcessTypeDto dto, string userId)
    {
        var entity = await _db.ProcessTypes.FindAsync(id)
            ?? throw new KeyNotFoundException($"Process type with ID {id} not found.");

        _mapper.Map(dto, entity);
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return _mapper.Map<ProcessTypeDto>(entity);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var entity = await _db.ProcessTypes.FindAsync(id)
            ?? throw new KeyNotFoundException($"Process type with ID {id} not found.");

        entity.IsDeleted = true;
        entity.UpdatedBy = userId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}

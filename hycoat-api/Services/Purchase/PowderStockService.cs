using AutoMapper;
using HycoatApi.Data;
using HycoatApi.DTOs.Purchase;
using HycoatApi.Models.Purchase;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Services.Purchase;

public class PowderStockService : IPowderStockService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public PowderStockService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<List<PowderStockDto>> GetAllAsync()
    {
        var stocks = await _db.PowderStocks
            .AsNoTracking()
            .Include(s => s.PowderColor)
            .OrderBy(s => s.PowderColor.PowderCode)
            .ToListAsync();

        return _mapper.Map<List<PowderStockDto>>(stocks);
    }

    public async Task<PowderStockDto> GetByPowderColorIdAsync(int powderColorId)
    {
        var stock = await _db.PowderStocks
            .AsNoTracking()
            .Include(s => s.PowderColor)
            .FirstOrDefaultAsync(s => s.PowderColorId == powderColorId)
            ?? throw new KeyNotFoundException($"Powder stock for color ID {powderColorId} not found.");

        return _mapper.Map<PowderStockDto>(stock);
    }

    public async Task<List<PowderStockDto>> GetLowStockAsync()
    {
        var stocks = await _db.PowderStocks
            .AsNoTracking()
            .Include(s => s.PowderColor)
            .Where(s => s.ReorderLevelKg.HasValue && s.CurrentStockKg < s.ReorderLevelKg.Value)
            .OrderBy(s => s.CurrentStockKg)
            .ToListAsync();

        return _mapper.Map<List<PowderStockDto>>(stocks);
    }

    public async Task<PowderStockDto> UpdateReorderLevelAsync(int powderColorId, UpdateReorderLevelDto dto)
    {
        var stock = await _db.PowderStocks
            .Include(s => s.PowderColor)
            .FirstOrDefaultAsync(s => s.PowderColorId == powderColorId);

        if (stock == null)
        {
            // Create stock record if it doesn't exist
            var powderColor = await _db.PowderColors.FindAsync(powderColorId)
                ?? throw new KeyNotFoundException($"Powder color with ID {powderColorId} not found.");

            stock = new PowderStock
            {
                PowderColorId = powderColorId,
                CurrentStockKg = 0,
                ReorderLevelKg = dto.ReorderLevelKg,
                LastUpdated = DateTime.UtcNow
            };
            _db.PowderStocks.Add(stock);
        }
        else
        {
            stock.ReorderLevelKg = dto.ReorderLevelKg;
            stock.LastUpdated = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        // Reload with navigation
        var result = await _db.PowderStocks
            .AsNoTracking()
            .Include(s => s.PowderColor)
            .FirstAsync(s => s.PowderColorId == powderColorId);

        return _mapper.Map<PowderStockDto>(result);
    }
}

using HycoatApi.DTOs.Purchase;

namespace HycoatApi.Services.Purchase;

public interface IPowderStockService
{
    Task<List<PowderStockDto>> GetAllAsync();
    Task<PowderStockDto> GetByPowderColorIdAsync(int powderColorId);
    Task<List<PowderStockDto>> GetLowStockAsync();
    Task<PowderStockDto> UpdateReorderLevelAsync(int powderColorId, UpdateReorderLevelDto dto);
}

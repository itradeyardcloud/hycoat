using HycoatApi.DTOs.Masters;

namespace HycoatApi.Services.Masters;

public interface IProductionUnitService
{
    Task<List<ProductionUnitDto>> GetAllAsync();
    Task<ProductionUnitDto> GetByIdAsync(int id);
    Task<ProductionUnitDto> CreateAsync(CreateProductionUnitDto dto, string userId);
    Task<ProductionUnitDto> UpdateAsync(int id, UpdateProductionUnitDto dto, string userId);
    Task DeleteAsync(int id, string userId);
}

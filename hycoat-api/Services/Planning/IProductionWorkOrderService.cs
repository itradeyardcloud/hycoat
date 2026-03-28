using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Planning;

namespace HycoatApi.Services.Planning;

public interface IProductionWorkOrderService
{
    Task<PagedResponse<ProductionWorkOrderDto>> GetAllAsync(
        string? search, string? status, int? workOrderId,
        DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<ProductionWorkOrderDetailDto> GetByIdAsync(int id);
    Task<ProductionWorkOrderDto> CreateAsync(CreateProductionWorkOrderDto dto, string userId);
    Task<ProductionWorkOrderDto> UpdateAsync(int id, UpdateProductionWorkOrderDto dto, string userId);
    Task UpdateStatusAsync(int id, UpdatePWOStatusDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<ProductionTimeCalcResultDto> CalculateTimeAsync(ProductionTimeCalcRequestDto dto);
    Task<List<LookupDto>> GetLookupAsync(string? status);
}

using HycoatApi.DTOs;
using HycoatApi.DTOs.Purchase;

namespace HycoatApi.Services.Purchase;

public interface IGRNService
{
    Task<PagedResponse<GRNDto>> GetAllAsync(
        string? search, int? purchaseOrderId, DateTime? dateFrom, DateTime? dateTo,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<GRNDetailDto> GetByIdAsync(int id);
    Task<GRNDto> CreateAsync(CreateGRNDto dto, string userId);
    Task<GRNDto> UpdateAsync(int id, CreateGRNDto dto, string userId);
    Task DeleteAsync(int id, string userId);
}

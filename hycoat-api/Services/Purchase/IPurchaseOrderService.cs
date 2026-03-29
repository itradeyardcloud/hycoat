using HycoatApi.DTOs;
using HycoatApi.DTOs.Purchase;

namespace HycoatApi.Services.Purchase;

public interface IPurchaseOrderService
{
    Task<PagedResponse<PurchaseOrderDto>> GetAllAsync(
        string? search, string? status, int? vendorId, DateTime? dateFrom, DateTime? dateTo,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<PurchaseOrderDetailDto> GetByIdAsync(int id);
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderDto dto, string userId);
    Task<PurchaseOrderDto> UpdateAsync(int id, CreatePurchaseOrderDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<PurchaseOrderDto> UpdateStatusAsync(int id, UpdatePurchaseOrderStatusDto dto, string userId);
    Task<byte[]> GeneratePdfAsync(int id);
}

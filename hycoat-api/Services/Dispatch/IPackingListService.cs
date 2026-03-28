using HycoatApi.DTOs;
using HycoatApi.DTOs.Dispatch;

namespace HycoatApi.Services.Dispatch;

public interface IPackingListService
{
    Task<PagedResponse<PackingListDto>> GetAllAsync(
        string? search, int? workOrderId, int page, int pageSize, string sortBy, bool sortDesc);
    Task<PackingListDetailDto> GetByIdAsync(int id);
    Task<PackingListDetailDto?> GetByWorkOrderIdAsync(int woId);
    Task<PackingListDto> CreateAsync(CreatePackingListDto dto, string userId);
    Task<PackingListDto> UpdateAsync(int id, CreatePackingListDto dto, string userId);
    Task DeleteAsync(int id, string userId);
}

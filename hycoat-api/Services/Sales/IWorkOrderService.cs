using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.Sales;

namespace HycoatApi.Services.Sales;

public interface IWorkOrderService
{
    Task<PagedResponse<WorkOrderDto>> GetAllAsync(string? search, string? status, int? customerId,
        int? processTypeId, int? powderColorId, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<WorkOrderDetailDto> GetByIdAsync(int id);
    Task<WorkOrderDto> CreateAsync(CreateWorkOrderDto dto, string userId);
    Task<WorkOrderDto> UpdateAsync(int id, CreateWorkOrderDto dto, string userId);
    Task UpdateStatusAsync(int id, UpdateWorkOrderStatusDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<WorkOrderStatsDto> GetStatsAsync();
    Task<WorkOrderTimelineDto> GetTimelineAsync(int id);
    Task<List<LookupDto>> GetLookupAsync();
}

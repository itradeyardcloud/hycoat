using HycoatApi.DTOs;
using HycoatApi.DTOs.Dispatch;

namespace HycoatApi.Services.Dispatch;

public interface IDeliveryChallanService
{
    Task<PagedResponse<DeliveryChallanDto>> GetAllAsync(
        string? search, DateTime? date, int? customerId, int? workOrderId, string? status,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<DeliveryChallanDetailDto> GetByIdAsync(int id);
    Task<DeliveryChallanDto> CreateAsync(CreateDeliveryChallanDto dto, string userId);
    Task<DeliveryChallanDto> UpdateAsync(int id, CreateDeliveryChallanDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<DeliveryChallanDto> UpdateStatusAsync(int id, string status, string userId);
    Task<byte[]> GeneratePdfAsync(int id);
    Task<byte[]?> GetPdfAsync(int id);
    Task UploadLoadingPhotosAsync(int id, List<IFormFile> files, string userId);
}

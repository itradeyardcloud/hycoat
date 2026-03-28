using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.MaterialInward;

namespace HycoatApi.Services.MaterialInward;

public interface IMaterialInwardService
{
    Task<PagedResponse<MaterialInwardDto>> GetAllAsync(string? search, string? status,
        int? customerId, int? workOrderId, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<MaterialInwardDetailDto> GetByIdAsync(int id);
    Task<MaterialInwardDto> CreateAsync(CreateMaterialInwardDto dto, string userId);
    Task<MaterialInwardDto> UpdateAsync(int id, UpdateMaterialInwardDto dto, string userId);
    Task UpdateStatusAsync(int id, UpdateMaterialInwardStatusDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<List<LookupDto>> GetLookupAsync(bool? hasInspection = null);
    Task<List<FileAttachmentDto>> UploadPhotosAsync(int id, List<IFormFile> files, string userId);
    Task<List<FileAttachmentDto>> GetPhotosAsync(int id);
    Task DeletePhotoAsync(int id, int photoId, string userId);
    Task<List<WorkOrderLookupDto>> GetWorkOrderLookupAsync(string? search);
}

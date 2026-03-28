using HycoatApi.DTOs;
using HycoatApi.DTOs.Common;
using HycoatApi.DTOs.MaterialInward;

namespace HycoatApi.Services.MaterialInward;

public interface IIncomingInspectionService
{
    Task<PagedResponse<IncomingInspectionDto>> GetAllAsync(string? search, string? overallStatus,
        int? materialInwardId, DateTime? fromDate, DateTime? toDate,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<IncomingInspectionDetailDto> GetByIdAsync(int id);
    Task<IncomingInspectionDto> CreateAsync(CreateIncomingInspectionDto dto, string userId);
    Task<IncomingInspectionDto> UpdateAsync(int id, CreateIncomingInspectionDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<List<FileAttachmentDto>> UploadPhotosAsync(int id, List<IFormFile> files, string userId);
    Task<List<FileAttachmentDto>> GetPhotosAsync(int id);
}

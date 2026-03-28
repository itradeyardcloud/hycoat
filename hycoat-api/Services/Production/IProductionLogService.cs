using HycoatApi.DTOs;
using HycoatApi.DTOs.Production;

namespace HycoatApi.Services.Production;

public interface IProductionLogService
{
    Task<PagedResponse<ProductionLogDto>> GetAllAsync(
        string? search, DateTime? date, string? shift, int? productionWorkOrderId,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<ProductionLogDetailDto> GetByIdAsync(int id);
    Task<List<ProductionLogDto>> GetByPWOAsync(int pwoId);
    Task<ProductionLogDto> CreateAsync(CreateProductionLogDto dto, string userId);
    Task<ProductionLogDto> UpdateAsync(int id, CreateProductionLogDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task<ProductionPhotoDto> UploadPhotoAsync(int id, IFormFile file, string? description, string userId);
    Task DeletePhotoAsync(int logId, int photoId, string userId);
}

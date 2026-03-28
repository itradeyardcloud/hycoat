using HycoatApi.DTOs;
using HycoatApi.DTOs.Quality;

namespace HycoatApi.Services.Quality;

public interface IFinalInspectionService
{
    Task<PagedResponse<FinalInspectionDto>> GetAllAsync(
        string? search, DateTime? date, int? productionWorkOrderId, string? overallStatus,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<FinalInspectionDetailDto> GetByIdAsync(int id);
    Task<FinalInspectionDetailDto?> GetByPWOAsync(int pwoId);
    Task<FinalInspectionDto> CreateAsync(CreateFinalInspectionDto dto, string userId);
    Task<FinalInspectionDto> UpdateAsync(int id, CreateFinalInspectionDto dto, string userId);
    Task DeleteAsync(int id, string userId);
}

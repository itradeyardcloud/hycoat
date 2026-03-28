using HycoatApi.DTOs;
using HycoatApi.DTOs.Quality;

namespace HycoatApi.Services.Quality;

public interface IInProcessInspectionService
{
    Task<PagedResponse<InProcessInspectionDto>> GetAllAsync(
        string? search, DateTime? date, int? productionWorkOrderId, string? inspectorUserId,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<InProcessInspectionDetailDto> GetByIdAsync(int id);
    Task<List<InProcessInspectionDto>> GetByPWOAsync(int pwoId);
    Task<List<DFTTrendDto>> GetDFTTrendAsync(int pwoId);
    Task<InProcessInspectionDto> CreateAsync(CreateInProcessInspectionDto dto, string userId);
    Task<InProcessInspectionDto> UpdateAsync(int id, CreateInProcessInspectionDto dto, string userId);
    Task DeleteAsync(int id, string userId);
}

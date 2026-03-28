using HycoatApi.DTOs;
using HycoatApi.DTOs.Quality;

namespace HycoatApi.Services.Quality;

public interface IPanelTestService
{
    Task<PagedResponse<PanelTestDto>> GetAllAsync(
        string? search, DateTime? date, int? productionWorkOrderId,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<PanelTestDetailDto> GetByIdAsync(int id);
    Task<List<PanelTestDto>> GetByPWOAsync(int pwoId);
    Task<PanelTestDto> CreateAsync(CreatePanelTestDto dto, string userId);
    Task<PanelTestDto> UpdateAsync(int id, CreatePanelTestDto dto, string userId);
    Task DeleteAsync(int id, string userId);
}

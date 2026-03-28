using HycoatApi.DTOs;
using HycoatApi.DTOs.Production;

namespace HycoatApi.Services.Production;

public interface IPretreatmentLogService
{
    Task<PagedResponse<PretreatmentLogDto>> GetAllAsync(
        string? search, DateTime? date, string? shift, int? productionWorkOrderId,
        int page, int pageSize, string sortBy, bool sortDesc);
    Task<PretreatmentLogDetailDto> GetByIdAsync(int id);
    Task<List<PretreatmentLogDto>> GetByPWOAsync(int pwoId);
    Task<PretreatmentLogDto> CreateAsync(CreatePretreatmentLogDto dto, string userId);
    Task<PretreatmentLogDto> UpdateAsync(int id, CreatePretreatmentLogDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task AddTankReadingsAsync(int id, List<TankReadingDto> readings, string userId);
}

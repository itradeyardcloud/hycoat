using HycoatApi.DTOs.Planning;

namespace HycoatApi.Services.Planning;

public interface IProductionScheduleService
{
    Task<List<ScheduleEntryDto>> GetScheduleAsync(
        DateTime startDate, DateTime endDate, int? productionUnitId, string? shift);
    Task<ScheduleEntryDto> CreateAsync(CreateScheduleEntryDto dto, string userId);
    Task<ScheduleEntryDto> UpdateAsync(int id, UpdateScheduleEntryDto dto, string userId);
    Task UpdateStatusAsync(int id, UpdateScheduleStatusDto dto, string userId);
    Task DeleteAsync(int id, string userId);
    Task ReorderAsync(ReorderScheduleDto dto, string userId);
}

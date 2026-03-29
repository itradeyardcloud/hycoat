using HycoatApi.DTOs;
using HycoatApi.DTOs.Reports;

namespace HycoatApi.Services.Reports;

public interface IYieldReportService
{
    Task<PagedResponse<YieldReportDto>> GetAllAsync(
        DateTime? dateFrom, DateTime? dateTo, int? productionUnitId,
        int page = 1, int pageSize = 20);

    Task<YieldSummaryDto> GetSummaryAsync(DateTime? date = null);

    Task<YieldReportDto> CreateAsync(CreateYieldReportDto dto, string userId);

    Task DeleteAsync(int id, string userId);
}
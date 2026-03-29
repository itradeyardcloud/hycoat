using HycoatApi.DTOs;
using HycoatApi.DTOs.Reports;

namespace HycoatApi.Services.Reports;

public interface IReportService
{
    Task<PagedResponse<OrderTrackerDto>> GetOrderTrackerAsync(
        DateTime? dateFrom, DateTime? dateTo, int? customerId, string? search,
        int page = 1, int pageSize = 20);

    Task<List<OrderTrackerDto>> ExportOrderTrackerAsync(
        DateTime? dateFrom, DateTime? dateTo, int? customerId, string? search);

    Task<ThroughputSummaryDto> GetProductionThroughputAsync(
        DateTime? dateFrom, DateTime? dateTo, int? customerId);

    Task<List<ProductionThroughputDto>> ExportProductionThroughputAsync(
        DateTime? dateFrom, DateTime? dateTo, int? customerId);

    Task<List<PowderConsumptionDto>> GetPowderConsumptionAsync(
        DateTime? dateFrom, DateTime? dateTo);

    Task<QualitySummaryDto> GetQualitySummaryAsync(
        DateTime? dateFrom, DateTime? dateTo);

    Task<CustomerHistoryDto> GetCustomerHistoryAsync(int customerId);

    Task<PagedResponse<DispatchRegisterDto>> GetDispatchRegisterAsync(
        DateTime? dateFrom, DateTime? dateTo, int? customerId, string? search,
        int page = 1, int pageSize = 20);

    Task<List<DispatchRegisterDto>> ExportDispatchRegisterAsync(
        DateTime? dateFrom, DateTime? dateTo, int? customerId, string? search);
}

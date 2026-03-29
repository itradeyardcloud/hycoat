using HycoatApi.DTOs.Dashboard;

namespace HycoatApi.Services.Dashboard;

public interface IDashboardService
{
    Task<AdminDashboardDto> GetAdminDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period);
    Task<AdminDashboardDto> GetLeaderDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period, string? department);
    Task<SalesDashboardDto> GetSalesDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period);
    Task<PPCDashboardDto> GetPPCDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period);
    Task<ProductionDashboardDto> GetProductionDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period);
    Task<QualityDashboardDto> GetQualityDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period);
    Task<SCMDashboardDto> GetSCMDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period);
    Task<PurchaseDashboardDto> GetPurchaseDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period);
    Task<FinanceDashboardDto> GetFinanceDashboardAsync(DateTime? dateFrom, DateTime? dateTo, string? period);
}

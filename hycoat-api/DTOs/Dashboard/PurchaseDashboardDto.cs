namespace HycoatApi.DTOs.Dashboard;

public class PurchaseDashboardDto
{
    public int OpenIndents { get; set; }
    public int PendingPOs { get; set; }
    public int LowStockItems { get; set; }
    public decimal TotalPowderStockKg { get; set; }
    public List<DashboardPowderStockDto> LowStockAlerts { get; set; } = [];
    public List<ChartPointDto> MonthlyPurchaseSpend { get; set; } = [];
}

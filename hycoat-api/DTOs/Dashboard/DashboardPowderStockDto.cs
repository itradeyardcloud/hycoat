namespace HycoatApi.DTOs.Dashboard;

public class DashboardPowderStockDto
{
    public string PowderCode { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public decimal CurrentStockKg { get; set; }
    public decimal? ReorderLevelKg { get; set; }
}

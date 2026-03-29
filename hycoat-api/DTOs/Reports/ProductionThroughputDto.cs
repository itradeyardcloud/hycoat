using HycoatApi.DTOs.Dashboard;

namespace HycoatApi.DTOs.Reports;

public class ProductionThroughputDto
{
    public DateTime Date { get; set; }
    public string Shift { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string PWONumber { get; set; } = string.Empty;
    public decimal AreaSFT { get; set; }
    public decimal PowderUsedKg { get; set; }
    public int BasketsProcessed { get; set; }
}

public class ThroughputSummaryDto
{
    public decimal TotalSFT { get; set; }
    public decimal TotalPowderKg { get; set; }
    public int TotalBaskets { get; set; }
    public List<ProductionThroughputDto> Details { get; set; } = [];
    public List<ChartPointDto> DailyTrend { get; set; } = [];
}

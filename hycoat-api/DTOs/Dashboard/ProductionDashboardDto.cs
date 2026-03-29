namespace HycoatApi.DTOs.Dashboard;

public class ProductionDashboardDto
{
    public int BasketsProcessedToday { get; set; }
    public decimal SFTCoatedToday { get; set; }
    public int ActiveProductionLogs { get; set; }
    public decimal AvgConveyorSpeed { get; set; }
    public List<ChartPointDto> DailyOutput { get; set; } = [];
}

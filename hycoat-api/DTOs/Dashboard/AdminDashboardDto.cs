namespace HycoatApi.DTOs.Dashboard;

public class AdminDashboardDto
{
    // KPI Cards
    public int ActiveWorkOrders { get; set; }
    public int PendingInquiries { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal MonthlyProductionSFT { get; set; }
    public int PendingDispatches { get; set; }
    public decimal QualityPassRate { get; set; }
    public int LowStockAlerts { get; set; }
    public int OverdueWorkOrders { get; set; }

    // Chart Data
    public List<ChartPointDto> RevenueTrend { get; set; } = [];
    public List<ChartPointDto> ProductionThroughput { get; set; } = [];
    public List<StatusCountDto> WorkOrdersByStatus { get; set; } = [];
    public List<CustomerRevenueDto> TopCustomers { get; set; } = [];
}

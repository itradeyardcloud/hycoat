namespace HycoatApi.DTOs.Dashboard;

public class PPCDashboardDto
{
    public int ActivePWOs { get; set; }
    public int UnscheduledPWOs { get; set; }
    public decimal WeekUtilizationPercent { get; set; }
    public decimal TotalScheduledSFT { get; set; }
    public List<ChartPointDto> WeeklyScheduleLoad { get; set; } = [];
    public List<StatusCountDto> PWOsByStatus { get; set; } = [];
}

namespace HycoatApi.DTOs.Dashboard;

public class SCMDashboardDto
{
    public int MaterialsReceivedToday { get; set; }
    public int PendingDispatches { get; set; }
    public int ChallansDraftedToday { get; set; }
    public int DispatchedThisWeek { get; set; }
    public List<StatusCountDto> DCsByStatus { get; set; } = [];
}

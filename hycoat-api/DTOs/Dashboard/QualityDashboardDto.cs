namespace HycoatApi.DTOs.Dashboard;

public class QualityDashboardDto
{
    public int InspectionsToday { get; set; }
    public decimal DFTPassRate { get; set; }
    public int PendingFinalInspections { get; set; }
    public int TestCertificatesIssued { get; set; }
    public decimal OverallPassRate { get; set; }
    public List<ChartPointDto> DFTTrend { get; set; } = [];
    public List<StatusCountDto> InspectionResults { get; set; } = [];
}

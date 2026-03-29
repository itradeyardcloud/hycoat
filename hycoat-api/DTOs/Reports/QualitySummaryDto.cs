using HycoatApi.DTOs.Dashboard;

namespace HycoatApi.DTOs.Reports;

public class QualitySummaryDto
{
    public int TotalInspections { get; set; }
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public int ReworkCount { get; set; }
    public decimal PassRate { get; set; }
    public decimal AvgDFT { get; set; }
    public List<ChartPointDto> DFTTrend { get; set; } = [];
    public List<StatusCountDto> FailureReasons { get; set; } = [];
}

namespace HycoatApi.DTOs.Quality;

public class InProcessInspectionDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public string PWONumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? InspectorName { get; set; }
    public string? Remarks { get; set; }
    public decimal? DFTAvg { get; set; }
    public int TestCount { get; set; }
    public int TestPassCount { get; set; }
    public int TestFailCount { get; set; }
    public bool AllWithinSpec { get; set; }
}

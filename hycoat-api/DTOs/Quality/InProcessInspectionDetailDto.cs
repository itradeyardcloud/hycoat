namespace HycoatApi.DTOs.Quality;

public class InProcessInspectionDetailDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public string PWONumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? InspectorName { get; set; }
    public string? Remarks { get; set; }
    public List<DFTReadingDto> DFTReadings { get; set; } = [];
    public List<TestResultDto> TestResults { get; set; } = [];
}

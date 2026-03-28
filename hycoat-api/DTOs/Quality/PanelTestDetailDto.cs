namespace HycoatApi.DTOs.Quality;

public class PanelTestDetailDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public string PWONumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? BoilingWaterResult { get; set; }
    public string? BoilingWaterStatus { get; set; }
    public string? ImpactTestResult { get; set; }
    public string? ImpactTestStatus { get; set; }
    public string? ConicalMandrelResult { get; set; }
    public string? ConicalMandrelStatus { get; set; }
    public string? PencilHardnessResult { get; set; }
    public string? PencilHardnessStatus { get; set; }
    public string? InspectorName { get; set; }
    public string? Remarks { get; set; }
}

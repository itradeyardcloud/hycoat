namespace HycoatApi.DTOs.Quality;

public class CreatePanelTestDto
{
    public DateTime Date { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public string? BoilingWaterResult { get; set; }
    public string? BoilingWaterStatus { get; set; }
    public string? ImpactTestResult { get; set; }
    public string? ImpactTestStatus { get; set; }
    public string? ConicalMandrelResult { get; set; }
    public string? ConicalMandrelStatus { get; set; }
    public string? PencilHardnessResult { get; set; }
    public string? PencilHardnessStatus { get; set; }
    public string? Remarks { get; set; }
}

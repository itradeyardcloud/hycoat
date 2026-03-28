namespace HycoatApi.DTOs.Quality;

public class CreateFinalInspectionDto
{
    public DateTime Date { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public int LotQuantity { get; set; }
    public int SampledQuantity { get; set; }
    public string? VisualCheckStatus { get; set; }
    public string? DFTRecheckStatus { get; set; }
    public string? ShadeMatchFinalStatus { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

namespace HycoatApi.DTOs.Quality;

public class FinalInspectionDto
{
    public int Id { get; set; }
    public string InspectionNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string PWONumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int LotQuantity { get; set; }
    public int SampledQuantity { get; set; }
    public string? VisualCheckStatus { get; set; }
    public string? DFTRecheckStatus { get; set; }
    public string? ShadeMatchFinalStatus { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public string? InspectorName { get; set; }
    public bool HasTestCertificate { get; set; }
}

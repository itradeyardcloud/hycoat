namespace HycoatApi.DTOs.Quality;

public class TestCertificateDetailDto
{
    public int Id { get; set; }
    public string CertificateNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int FinalInspectionId { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int WorkOrderId { get; set; }
    public string WorkOrderNumber { get; set; } = string.Empty;
    public string? ProductCode { get; set; }
    public string? ProjectName { get; set; }
    public int LotQuantity { get; set; }
    public string? Warranty { get; set; }
    public string? SubstrateResult { get; set; }
    public string? BakingTempResult { get; set; }
    public string? BakingTimeResult { get; set; }
    public string? ColorResult { get; set; }
    public string? DFTResult { get; set; }
    public string? MEKResult { get; set; }
    public string? CrossCutResult { get; set; }
    public string? ConicalMandrelResult { get; set; }
    public string? BoilingWaterResult { get; set; }
    public string? FileUrl { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
}

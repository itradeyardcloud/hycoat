namespace HycoatApi.DTOs.Quality;

public class CreateTestCertificateDto
{
    public DateTime Date { get; set; }
    public int FinalInspectionId { get; set; }
    public int CustomerId { get; set; }
    public int WorkOrderId { get; set; }
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
}

using HycoatApi.Models.Common;
using HycoatApi.Models.Masters;
using HycoatApi.Models.Sales;

namespace HycoatApi.Models.Quality;

public class TestCertificate : BaseEntity
{
    public string CertificateNumber { get; set; } = string.Empty;
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
    public string? FileUrl { get; set; }

    // Navigation
    public FinalInspection FinalInspection { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public WorkOrder WorkOrder { get; set; } = null!;
}

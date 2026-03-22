using HycoatApi.Models.Common;
using HycoatApi.Models.Identity;
using HycoatApi.Models.Planning;

namespace HycoatApi.Models.Quality;

public class FinalInspection : BaseEntity
{
    public string InspectionNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public int LotQuantity { get; set; }
    public int SampledQuantity { get; set; }
    public string? VisualCheckStatus { get; set; }
    public string? DFTRecheckStatus { get; set; }
    public string? ShadeMatchFinalStatus { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public string? InspectorUserId { get; set; }
    public string? Remarks { get; set; }

    // Navigation
    public ProductionWorkOrder ProductionWorkOrder { get; set; } = null!;
    public AppUser? InspectorUser { get; set; }
    public TestCertificate? TestCertificate { get; set; }
}

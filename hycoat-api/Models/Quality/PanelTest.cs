using HycoatApi.Models.Common;
using HycoatApi.Models.Identity;
using HycoatApi.Models.Planning;

namespace HycoatApi.Models.Quality;

public class PanelTest : BaseEntity
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
    public string? InspectorUserId { get; set; }
    public string? Remarks { get; set; }

    // Navigation
    public ProductionWorkOrder ProductionWorkOrder { get; set; } = null!;
    public AppUser? InspectorUser { get; set; }
}

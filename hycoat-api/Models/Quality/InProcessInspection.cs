using HycoatApi.Models.Common;
using HycoatApi.Models.Identity;
using HycoatApi.Models.Planning;

namespace HycoatApi.Models.Quality;

public class InProcessInspection : BaseEntity
{
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public string? InspectorUserId { get; set; }
    public string? Remarks { get; set; }

    // Navigation
    public ProductionWorkOrder ProductionWorkOrder { get; set; } = null!;
    public AppUser? InspectorUser { get; set; }
    public ICollection<InProcessDFTReading> DFTReadings { get; set; } = new List<InProcessDFTReading>();
    public ICollection<InProcessTestResult> TestResults { get; set; } = new List<InProcessTestResult>();
}

using HycoatApi.Models.Common;
using HycoatApi.Models.Identity;

namespace HycoatApi.Models.MaterialInward;

public class IncomingInspection : BaseEntity
{
    public string InspectionNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int MaterialInwardId { get; set; }
    public string? InspectedByUserId { get; set; }
    public string OverallStatus { get; set; } = string.Empty;
    public string? Remarks { get; set; }

    // Navigation
    public MaterialInward MaterialInward { get; set; } = null!;
    public AppUser? InspectedByUser { get; set; }
    public ICollection<IncomingInspectionLine> Lines { get; set; } = new List<IncomingInspectionLine>();
}

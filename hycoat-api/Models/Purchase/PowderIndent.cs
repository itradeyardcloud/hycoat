using HycoatApi.Models.Common;
using HycoatApi.Models.Identity;
using HycoatApi.Models.Planning;

namespace HycoatApi.Models.Purchase;

public class PowderIndent : BaseEntity
{
    public string IndentNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int? ProductionWorkOrderId { get; set; }
    public string? RequestedByUserId { get; set; }
    public string Status { get; set; } = "Requested";
    public string? Notes { get; set; }

    // Navigation
    public ProductionWorkOrder? ProductionWorkOrder { get; set; }
    public AppUser? RequestedByUser { get; set; }
    public ICollection<PowderIndentLine> Lines { get; set; } = new List<PowderIndentLine>();
}

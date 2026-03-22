using HycoatApi.Models.Common;
using HycoatApi.Models.Identity;
using HycoatApi.Models.Planning;
using HycoatApi.Models.Sales;

namespace HycoatApi.Models.Dispatch;

public class PackingList : BaseEntity
{
    public DateTime Date { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public int WorkOrderId { get; set; }
    public string? PackingType { get; set; }
    public int? BundleCount { get; set; }
    public string? PreparedByUserId { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public ProductionWorkOrder ProductionWorkOrder { get; set; } = null!;
    public WorkOrder WorkOrder { get; set; } = null!;
    public AppUser? PreparedByUser { get; set; }
    public ICollection<PackingListLine> Lines { get; set; } = new List<PackingListLine>();
}

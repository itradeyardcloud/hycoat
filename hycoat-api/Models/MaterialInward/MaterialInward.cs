using HycoatApi.Models.Common;
using HycoatApi.Models.Identity;
using HycoatApi.Models.Masters;
using HycoatApi.Models.Sales;

namespace HycoatApi.Models.MaterialInward;

public class MaterialInward : BaseEntity
{
    public string InwardNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public int? WorkOrderId { get; set; }
    public string? CustomerDCNumber { get; set; }
    public DateTime? CustomerDCDate { get; set; }
    public string? VehicleNumber { get; set; }
    public string? UnloadingLocation { get; set; }
    public int? ProcessTypeId { get; set; }
    public int? PowderColorId { get; set; }
    public string? ReceivedByUserId { get; set; }
    public string Status { get; set; } = "Received";
    public string? Notes { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public WorkOrder? WorkOrder { get; set; }
    public ProcessType? ProcessType { get; set; }
    public PowderColor? PowderColor { get; set; }
    public AppUser? ReceivedByUser { get; set; }
    public ICollection<MaterialInwardLine> Lines { get; set; } = new List<MaterialInwardLine>();
    public ICollection<IncomingInspection> IncomingInspections { get; set; } = new List<IncomingInspection>();
}

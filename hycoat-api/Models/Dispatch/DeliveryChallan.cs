using HycoatApi.Models.Common;
using HycoatApi.Models.Masters;
using HycoatApi.Models.Sales;

namespace HycoatApi.Models.Dispatch;

public class DeliveryChallan : BaseEntity
{
    public string DCNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int WorkOrderId { get; set; }
    public int CustomerId { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerGSTIN { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? LRNumber { get; set; }
    public decimal? MaterialValueApprox { get; set; }
    public string Status { get; set; } = "Created";
    public string? Notes { get; set; }

    // Navigation
    public WorkOrder WorkOrder { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public ICollection<DCLineItem> LineItems { get; set; } = new List<DCLineItem>();
}

using HycoatApi.Models.Common;
using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Sales;

public class WorkOrder : BaseEntity
{
    public string WONumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public int? ProformaInvoiceId { get; set; }
    public string? ProjectName { get; set; }
    public int ProcessTypeId { get; set; }
    public int? PowderColorId { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string Status { get; set; } = "Created";
    public string? Notes { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public ProformaInvoice? ProformaInvoice { get; set; }
    public ProcessType ProcessType { get; set; } = null!;
    public PowderColor? PowderColor { get; set; }
    public ICollection<MaterialInward.MaterialInward> MaterialInwards { get; set; } = new List<MaterialInward.MaterialInward>();
    public ICollection<Planning.ProductionWorkOrder> ProductionWorkOrders { get; set; } = new List<Planning.ProductionWorkOrder>();
    public ICollection<Dispatch.DeliveryChallan> DeliveryChallans { get; set; } = new List<Dispatch.DeliveryChallan>();
    public ICollection<Dispatch.Invoice> Invoices { get; set; } = new List<Dispatch.Invoice>();
}

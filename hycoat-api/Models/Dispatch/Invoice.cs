using HycoatApi.Models.Common;
using HycoatApi.Models.Masters;
using HycoatApi.Models.Sales;

namespace HycoatApi.Models.Dispatch;

public class Invoice : BaseEntity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public int WorkOrderId { get; set; }
    public int? DeliveryChallanId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerGSTIN { get; set; }
    public string OurGSTIN { get; set; } = string.Empty;
    public string? HSNSACCode { get; set; }
    public decimal SubTotal { get; set; }
    public decimal PackingCharges { get; set; }
    public decimal TransportCharges { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal CGSTRate { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTRate { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTRate { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal RoundOff { get; set; }
    public string? AmountInWords { get; set; }
    public bool IsInterState { get; set; }
    public string? PaymentTerms { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNo { get; set; }
    public string? BankIFSC { get; set; }
    public string Status { get; set; } = "Draft";
    public string? FileUrl { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public WorkOrder WorkOrder { get; set; } = null!;
    public DeliveryChallan? DeliveryChallan { get; set; }
    public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
}

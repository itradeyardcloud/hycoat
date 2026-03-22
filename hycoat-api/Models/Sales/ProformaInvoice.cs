using HycoatApi.Models.Common;
using HycoatApi.Models.Identity;
using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Sales;

public class ProformaInvoice : BaseEntity
{
    public string PINumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public int? QuotationId { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerGSTIN { get; set; }
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
    public bool IsInterState { get; set; }
    public string Status { get; set; } = "Draft";
    public string? FileUrl { get; set; }
    public string? Notes { get; set; }
    public string? PreparedByUserId { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public Quotation? Quotation { get; set; }
    public AppUser? PreparedByUser { get; set; }
    public ICollection<PILineItem> LineItems { get; set; } = new List<PILineItem>();
}

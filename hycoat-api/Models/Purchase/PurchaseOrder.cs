using HycoatApi.Models.Common;
using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Purchase;

public class PurchaseOrder : BaseEntity
{
    public string PONumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int VendorId { get; set; }
    public int? PowderIndentId { get; set; }
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }

    // Navigation
    public Vendor Vendor { get; set; } = null!;
    public PowderIndent? PowderIndent { get; set; }
    public ICollection<POLineItem> LineItems { get; set; } = new List<POLineItem>();
}

using HycoatApi.Models.Common;

namespace HycoatApi.Models.Masters;

public class Vendor : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string VendorType { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? GSTIN { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }

    // Navigation collections
    public ICollection<PowderColor> PowderColors { get; set; } = new List<PowderColor>();
    public ICollection<Purchase.PurchaseOrder> PurchaseOrders { get; set; } = new List<Purchase.PurchaseOrder>();
}

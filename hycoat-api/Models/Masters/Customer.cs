using HycoatApi.Models.Common;

namespace HycoatApi.Models.Masters;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? GSTIN { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }

    // Navigation collections
    public ICollection<Sales.Inquiry> Inquiries { get; set; } = new List<Sales.Inquiry>();
    public ICollection<Sales.WorkOrder> WorkOrders { get; set; } = new List<Sales.WorkOrder>();
    public ICollection<MaterialInward.MaterialInward> MaterialInwards { get; set; } = new List<MaterialInward.MaterialInward>();
    public ICollection<Dispatch.Invoice> Invoices { get; set; } = new List<Dispatch.Invoice>();
}

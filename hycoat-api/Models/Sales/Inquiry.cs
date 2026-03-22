using HycoatApi.Models.Common;
using HycoatApi.Models.Identity;
using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Sales;

public class Inquiry : BaseEntity
{
    public string InquiryNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public string? ProjectName { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public int? ProcessTypeId { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = "New";
    public string? AssignedToUserId { get; set; }

    // Navigation
    public Customer Customer { get; set; } = null!;
    public ProcessType? ProcessType { get; set; }
    public AppUser? AssignedToUser { get; set; }
    public ICollection<Quotation> Quotations { get; set; } = new List<Quotation>();
}

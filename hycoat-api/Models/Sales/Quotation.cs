using HycoatApi.Models.Common;
using HycoatApi.Models.Identity;
using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Sales;

public class Quotation : BaseEntity
{
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int? InquiryId { get; set; }
    public int CustomerId { get; set; }
    public int ValidityDays { get; set; } = 30;
    public string Status { get; set; } = "Draft";
    public string? Notes { get; set; }
    public string? FileUrl { get; set; }
    public string? PreparedByUserId { get; set; }

    // Navigation
    public Inquiry? Inquiry { get; set; }
    public Customer Customer { get; set; } = null!;
    public AppUser? PreparedByUser { get; set; }
    public ICollection<QuotationLineItem> LineItems { get; set; } = new List<QuotationLineItem>();
}

namespace HycoatApi.DTOs.Sales;

public class InquiryDetailDto : InquiryDto
{
    public int CustomerId { get; set; }
    public int? ProcessTypeId { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Description { get; set; }
    public string? AssignedToUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int QuotationCount { get; set; }
}

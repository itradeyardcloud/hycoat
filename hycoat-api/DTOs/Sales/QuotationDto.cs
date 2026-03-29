namespace HycoatApi.DTOs.Sales;

public class QuotationDto
{
    public int Id { get; set; }
    public string QuotationNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? InquiryNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ValidityDays { get; set; }
    public int LineItemCount { get; set; }
}

namespace HycoatApi.DTOs.Sales;

public class CreateQuotationDto
{
    public DateTime Date { get; set; }
    public int? InquiryId { get; set; }
    public int CustomerId { get; set; }
    public int ValidityDays { get; set; } = 30;
    public string? Notes { get; set; }
    public List<CreateQuotationLineItemDto> LineItems { get; set; } = [];
}

public class CreateQuotationLineItemDto
{
    public int ProcessTypeId { get; set; }
    public string? Description { get; set; }
    public decimal RatePerSFT { get; set; }
    public int? WarrantyYears { get; set; }
    public string? MicronRange { get; set; }
}

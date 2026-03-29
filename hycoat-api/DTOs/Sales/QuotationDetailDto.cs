namespace HycoatApi.DTOs.Sales;

public class QuotationDetailDto : QuotationDto
{
    public int CustomerId { get; set; }
    public int? InquiryId { get; set; }
    public string? Notes { get; set; }
    public string? FileUrl { get; set; }
    public string? PreparedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<QuotationLineItemDetailDto> LineItems { get; set; } = [];
}

public class QuotationLineItemDetailDto
{
    public int Id { get; set; }
    public int ProcessTypeId { get; set; }
    public string ProcessTypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal RatePerSFT { get; set; }
    public int? WarrantyYears { get; set; }
    public string? MicronRange { get; set; }
}

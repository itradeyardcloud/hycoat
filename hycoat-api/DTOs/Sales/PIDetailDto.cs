namespace HycoatApi.DTOs.Sales;

public class PIDetailDto : PIDto
{
    public int CustomerId { get; set; }
    public string? CustomerAddress { get; set; }
    public string? CustomerGSTIN { get; set; }
    public int? QuotationId { get; set; }
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
    public bool IsInterState { get; set; }
    public string? FileUrl { get; set; }
    public string? Notes { get; set; }
    public string? PreparedByName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PILineItemDetailDto> LineItems { get; set; } = [];
}

public class PILineItemDetailDto
{
    public int Id { get; set; }
    public int SectionProfileId { get; set; }
    public string SectionNumber { get; set; } = string.Empty;
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public decimal PerimeterMM { get; set; }
    public decimal AreaSFT { get; set; }
    public decimal RatePerSFT { get; set; }
    public decimal Amount { get; set; }
}

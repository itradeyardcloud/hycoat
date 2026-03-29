namespace HycoatApi.DTOs.Sales;

public class CreateProformaInvoiceDto
{
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public int? QuotationId { get; set; }
    public decimal PackingCharges { get; set; }
    public decimal TransportCharges { get; set; }
    public bool IsInterState { get; set; }
    public string? Notes { get; set; }
    public List<CreatePILineItemDto> LineItems { get; set; } = [];
}

public class CreatePILineItemDto
{
    public int SectionProfileId { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public decimal RatePerSFT { get; set; }
}

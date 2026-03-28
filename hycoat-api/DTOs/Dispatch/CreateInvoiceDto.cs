namespace HycoatApi.DTOs.Dispatch;

public class CreateInvoiceDto
{
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public int WorkOrderId { get; set; }
    public int? DeliveryChallanId { get; set; }
    public string? HSNSACCode { get; set; }
    public decimal PackingCharges { get; set; }
    public decimal TransportCharges { get; set; }
    public bool IsInterState { get; set; }
    public decimal CGSTRate { get; set; }
    public decimal SGSTRate { get; set; }
    public decimal IGSTRate { get; set; }
    public string? PaymentTerms { get; set; }
    public List<CreateInvoiceLineItemDto> Lines { get; set; } = [];
}

public class CreateInvoiceLineItemDto
{
    public int SectionProfileId { get; set; }
    public string? DCNumber { get; set; }
    public string? Color { get; set; }
    public string? MicronRange { get; set; }
    public decimal PerimeterMM { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public decimal RatePerSFT { get; set; }
}

namespace HycoatApi.DTOs.Dispatch;

public class InvoiceAutoFillDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerAddress { get; set; }
    public string? CustomerGSTIN { get; set; }
    public int? DeliveryChallanId { get; set; }
    public string? DCNumber { get; set; }
    public List<InvoiceLineAutoFillDto> Lines { get; set; } = [];
}

public class InvoiceLineAutoFillDto
{
    public int SectionProfileId { get; set; }
    public string SectionNumber { get; set; } = string.Empty;
    public decimal PerimeterMM { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public decimal AreaSFT { get; set; }
    public string? Color { get; set; }
}

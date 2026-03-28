namespace HycoatApi.DTOs.Dispatch;

public class InvoiceDetailDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerAddress { get; set; }
    public string? CustomerGSTIN { get; set; }
    public string OurGSTIN { get; set; } = string.Empty;
    public int WorkOrderId { get; set; }
    public string WONumber { get; set; } = string.Empty;
    public int? DeliveryChallanId { get; set; }
    public string? DCNumber { get; set; }
    public string? HSNSACCode { get; set; }
    public decimal SubTotal { get; set; }
    public decimal PackingCharges { get; set; }
    public decimal TransportCharges { get; set; }
    public decimal TaxableAmount { get; set; }
    public bool IsInterState { get; set; }
    public decimal CGSTRate { get; set; }
    public decimal CGSTAmount { get; set; }
    public decimal SGSTRate { get; set; }
    public decimal SGSTAmount { get; set; }
    public decimal IGSTRate { get; set; }
    public decimal IGSTAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public decimal RoundOff { get; set; }
    public string? AmountInWords { get; set; }
    public string? PaymentTerms { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNo { get; set; }
    public string? BankIFSC { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public List<InvoiceLineItemDto> Lines { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class InvoiceLineItemDto
{
    public int Id { get; set; }
    public int SectionProfileId { get; set; }
    public string? SectionNumber { get; set; }
    public string? DCNumber { get; set; }
    public string? Color { get; set; }
    public string? MicronRange { get; set; }
    public decimal PerimeterMM { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public decimal AreaSFT { get; set; }
    public decimal RatePerSFT { get; set; }
    public decimal Amount { get; set; }
}

public class UpdateInvoiceStatusDto
{
    public string Status { get; set; } = string.Empty;
}

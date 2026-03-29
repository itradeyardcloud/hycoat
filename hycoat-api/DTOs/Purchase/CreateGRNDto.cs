namespace HycoatApi.DTOs.Purchase;

public class CreateGRNDto
{
    public DateTime Date { get; set; }
    public int PurchaseOrderId { get; set; }
    public string? Notes { get; set; }
    public List<CreateGRNLineItemDto> Lines { get; set; } = [];
}

public class CreateGRNLineItemDto
{
    public int PowderColorId { get; set; }
    public decimal QtyReceivedKg { get; set; }
    public string? BatchCode { get; set; }
    public DateTime? MfgDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

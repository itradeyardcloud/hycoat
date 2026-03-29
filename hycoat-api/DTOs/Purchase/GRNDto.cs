namespace HycoatApi.DTOs.Purchase;

public class GRNDto
{
    public int Id { get; set; }
    public string GRNNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public string? ReceivedByName { get; set; }
    public int LineCount { get; set; }
    public decimal TotalReceivedKg { get; set; }
}

public class GRNDetailDto : GRNDto
{
    public int PurchaseOrderId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<GRNLineItemDto> Lines { get; set; } = [];
}

public class GRNLineItemDto
{
    public int Id { get; set; }
    public int PowderColorId { get; set; }
    public string PowderCode { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string? RALCode { get; set; }
    public decimal QtyReceivedKg { get; set; }
    public string? BatchCode { get; set; }
    public DateTime? MfgDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

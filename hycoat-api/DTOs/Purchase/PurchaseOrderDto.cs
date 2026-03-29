namespace HycoatApi.DTOs.Purchase;

public class PurchaseOrderDto
{
    public int Id { get; set; }
    public string PONumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string? IndentNumber { get; set; }
    public int LineCount { get; set; }
    public decimal TotalQtyKg { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class PurchaseOrderDetailDto : PurchaseOrderDto
{
    public int VendorId { get; set; }
    public int? PowderIndentId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<POLineItemDto> Lines { get; set; } = [];
}

public class POLineItemDto
{
    public int Id { get; set; }
    public int PowderColorId { get; set; }
    public string PowderCode { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string? RALCode { get; set; }
    public decimal QtyKg { get; set; }
    public decimal RatePerKg { get; set; }
    public decimal Amount { get; set; }
    public DateTime? RequiredByDate { get; set; }
}

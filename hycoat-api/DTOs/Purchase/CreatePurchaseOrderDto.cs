namespace HycoatApi.DTOs.Purchase;

public class CreatePurchaseOrderDto
{
    public DateTime Date { get; set; }
    public int VendorId { get; set; }
    public int? PowderIndentId { get; set; }
    public string? Notes { get; set; }
    public List<CreatePOLineItemDto> Lines { get; set; } = [];
}

public class CreatePOLineItemDto
{
    public int PowderColorId { get; set; }
    public decimal QtyKg { get; set; }
    public decimal RatePerKg { get; set; }
    public DateTime? RequiredByDate { get; set; }
}

public class UpdatePurchaseOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
}

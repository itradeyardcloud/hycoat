using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Purchase;

public class POLineItem
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int PowderColorId { get; set; }
    public decimal QtyKg { get; set; }
    public decimal RatePerKg { get; set; }
    public decimal Amount { get; set; }
    public DateTime? RequiredByDate { get; set; }

    // Navigation
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public PowderColor PowderColor { get; set; } = null!;
}

using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Purchase;

public class GRNLineItem
{
    public int Id { get; set; }
    public int GoodsReceivedNoteId { get; set; }
    public int PowderColorId { get; set; }
    public decimal QtyReceivedKg { get; set; }
    public string? BatchCode { get; set; }
    public DateTime? MfgDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // Navigation
    public GoodsReceivedNote GoodsReceivedNote { get; set; } = null!;
    public PowderColor PowderColor { get; set; } = null!;
}

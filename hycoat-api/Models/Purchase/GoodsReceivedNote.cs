using HycoatApi.Models.Common;
using HycoatApi.Models.Identity;

namespace HycoatApi.Models.Purchase;

public class GoodsReceivedNote : BaseEntity
{
    public string GRNNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int PurchaseOrderId { get; set; }
    public string? ReceivedByUserId { get; set; }
    public string? Notes { get; set; }

    // Navigation
    public PurchaseOrder PurchaseOrder { get; set; } = null!;
    public AppUser? ReceivedByUser { get; set; }
    public ICollection<GRNLineItem> LineItems { get; set; } = new List<GRNLineItem>();
}

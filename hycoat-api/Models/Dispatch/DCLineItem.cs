using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Dispatch;

public class DCLineItem
{
    public int Id { get; set; }
    public int DeliveryChallanId { get; set; }
    public int SectionProfileId { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public string? CustomerDCRef { get; set; }
    public string? Remarks { get; set; }

    // Navigation
    public DeliveryChallan DeliveryChallan { get; set; } = null!;
    public SectionProfile SectionProfile { get; set; } = null!;
}

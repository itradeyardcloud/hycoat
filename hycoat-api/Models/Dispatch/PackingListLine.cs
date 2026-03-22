using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Dispatch;

public class PackingListLine
{
    public int Id { get; set; }
    public int PackingListId { get; set; }
    public int SectionProfileId { get; set; }
    public int Quantity { get; set; }
    public decimal LengthMM { get; set; }
    public int? BundleNumber { get; set; }
    public string? Remarks { get; set; }

    // Navigation
    public PackingList PackingList { get; set; } = null!;
    public SectionProfile SectionProfile { get; set; } = null!;
}

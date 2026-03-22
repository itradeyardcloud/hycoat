using HycoatApi.Models.Masters;

namespace HycoatApi.Models.MaterialInward;

public class MaterialInwardLine
{
    public int Id { get; set; }
    public int MaterialInwardId { get; set; }
    public int SectionProfileId { get; set; }
    public decimal LengthMM { get; set; }
    public int QtyAsPerDC { get; set; }
    public int QtyReceived { get; set; }
    public decimal? WeightKg { get; set; }
    public int Discrepancy { get; set; }
    public string? Remarks { get; set; }

    // Navigation
    public MaterialInward MaterialInward { get; set; } = null!;
    public SectionProfile SectionProfile { get; set; } = null!;
}

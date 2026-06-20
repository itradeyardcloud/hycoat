using HycoatApi.Models.Masters;

namespace HycoatApi.Models.MaterialInward;

public class MaterialInwardPowderColor
{
    public int Id { get; set; }
    public int MaterialInwardId { get; set; }
    public int PowderColorId { get; set; }

    // Navigation
    public MaterialInward MaterialInward { get; set; } = null!;
    public PowderColor PowderColor { get; set; } = null!;
}

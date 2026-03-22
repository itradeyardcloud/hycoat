using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Purchase;

public class PowderIndentLine
{
    public int Id { get; set; }
    public int PowderIndentId { get; set; }
    public int PowderColorId { get; set; }
    public decimal RequiredQtyKg { get; set; }

    // Navigation
    public PowderIndent PowderIndent { get; set; } = null!;
    public PowderColor PowderColor { get; set; } = null!;
}

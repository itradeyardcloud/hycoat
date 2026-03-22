using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Purchase;

public class PowderStock
{
    public int Id { get; set; }
    public int PowderColorId { get; set; }
    public decimal CurrentStockKg { get; set; }
    public decimal? ReorderLevelKg { get; set; }
    public DateTime LastUpdated { get; set; }

    // Navigation
    public PowderColor PowderColor { get; set; } = null!;
}

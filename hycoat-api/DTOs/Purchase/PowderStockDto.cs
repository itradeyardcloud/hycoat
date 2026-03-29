namespace HycoatApi.DTOs.Purchase;

public class PowderStockDto
{
    public int Id { get; set; }
    public int PowderColorId { get; set; }
    public string PowderCode { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string? RALCode { get; set; }
    public decimal CurrentStockKg { get; set; }
    public decimal? ReorderLevelKg { get; set; }
    public bool IsBelowReorderLevel { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class UpdateReorderLevelDto
{
    public decimal ReorderLevelKg { get; set; }
}

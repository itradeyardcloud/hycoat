namespace HycoatApi.DTOs.Reports;

public class PowderConsumptionDto
{
    public string PowderCode { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public decimal OrderedKg { get; set; }
    public decimal ReceivedKg { get; set; }
    public decimal ConsumedKg { get; set; }
    public decimal CurrentStockKg { get; set; }
    public decimal WastagePercent { get; set; }
}

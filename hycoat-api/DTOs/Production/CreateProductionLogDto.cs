namespace HycoatApi.DTOs.Production;

public class CreateProductionLogDto
{
    public DateTime Date { get; set; }
    public string Shift { get; set; } = string.Empty;
    public int ProductionWorkOrderId { get; set; }
    public decimal? ConveyorSpeedMtrPerMin { get; set; }
    public decimal? OvenTemperature { get; set; }
    public string? PowderBatchNo { get; set; }
    public string? Remarks { get; set; }
}

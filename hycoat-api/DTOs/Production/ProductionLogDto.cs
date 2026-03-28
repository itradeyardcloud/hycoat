namespace HycoatApi.DTOs.Production;

public class ProductionLogDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Shift { get; set; } = string.Empty;
    public string PWONumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal? ConveyorSpeedMtrPerMin { get; set; }
    public decimal? OvenTemperature { get; set; }
    public string? PowderBatchNo { get; set; }
    public string? SupervisorName { get; set; }
    public int PhotoCount { get; set; }
}

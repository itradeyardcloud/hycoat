namespace HycoatApi.DTOs.Production;

public class CreatePretreatmentLogDto
{
    public DateTime Date { get; set; }
    public string Shift { get; set; } = string.Empty;
    public int ProductionWorkOrderId { get; set; }
    public int BasketNumber { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public decimal? EtchTimeMins { get; set; }
    public string? Remarks { get; set; }
    public List<TankReadingDto> TankReadings { get; set; } = new();
}

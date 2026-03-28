namespace HycoatApi.DTOs.Production;

public class PretreatmentLogDetailDto : PretreatmentLogDto
{
    public int ProductionWorkOrderId { get; set; }
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TankReadingDto> TankReadings { get; set; } = new();
}

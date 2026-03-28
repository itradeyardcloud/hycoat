namespace HycoatApi.DTOs.Planning;

public class ReorderScheduleDto
{
    public DateTime Date { get; set; }
    public string Shift { get; set; } = string.Empty;
    public int ProductionUnitId { get; set; }
    public List<int> ScheduleIds { get; set; } = new();
}

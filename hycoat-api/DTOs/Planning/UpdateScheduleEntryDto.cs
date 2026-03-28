namespace HycoatApi.DTOs.Planning;

public class UpdateScheduleEntryDto
{
    public DateTime Date { get; set; }
    public string Shift { get; set; } = string.Empty;
    public int ProductionWorkOrderId { get; set; }
    public int ProductionUnitId { get; set; }
    public string? Notes { get; set; }
}

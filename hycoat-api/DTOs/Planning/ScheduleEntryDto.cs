namespace HycoatApi.DTOs.Planning;

public class ScheduleEntryDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Shift { get; set; } = string.Empty;
    public int ProductionUnitId { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public int ProductionWorkOrderId { get; set; }
    public string PWONumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? PowderColor { get; set; }
    public decimal? TotalTimeHrs { get; set; }
    public int SortOrder { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

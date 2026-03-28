namespace HycoatApi.DTOs.Planning;

public class ProductionWorkOrderDto
{
    public int Id { get; set; }
    public string PWONumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string WONumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? PowderCode { get; set; }
    public string? ColorName { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;
    public string? ShiftAllocation { get; set; }
    public decimal? TotalTimeHrs { get; set; }
    public string Status { get; set; } = string.Empty;
}

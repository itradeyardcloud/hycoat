namespace HycoatApi.DTOs.Sales;

public class WorkOrderDto
{
    public int Id { get; set; }
    public string WONumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
    public string ProcessTypeName { get; set; } = string.Empty;
    public string? PowderColorName { get; set; }
    public string? PowderCode { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? DispatchDate { get; set; }
}

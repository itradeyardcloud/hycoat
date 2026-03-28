namespace HycoatApi.DTOs.Dispatch;

public class PackingListDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public int WorkOrderId { get; set; }
    public string WONumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int ProductionWorkOrderId { get; set; }
    public string PWONumber { get; set; } = string.Empty;
    public string? PackingType { get; set; }
    public int? BundleCount { get; set; }
    public string? PreparedByName { get; set; }
    public int LineCount { get; set; }
    public int TotalQuantity { get; set; }
}

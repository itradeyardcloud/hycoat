namespace HycoatApi.DTOs.Dispatch;

public class DeliveryChallanDto
{
    public int Id { get; set; }
    public string DCNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string WONumber { get; set; } = string.Empty;
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public int TotalQuantity { get; set; }
}

namespace HycoatApi.DTOs.Reports;

public class DispatchRegisterDto
{
    public string DCNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string WorkOrderNumber { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public string? VehicleNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
}

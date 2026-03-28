namespace HycoatApi.DTOs.MaterialInward;

public class MaterialInwardDto
{
    public int Id { get; set; }
    public string InwardNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int? WorkOrderId { get; set; }
    public string? WONumber { get; set; }
    public string? VehicleNumber { get; set; }
    public string? CustomerDCNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public int LineCount { get; set; }
    public bool HasPhotos { get; set; }
}

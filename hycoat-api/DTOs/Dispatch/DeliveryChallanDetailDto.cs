namespace HycoatApi.DTOs.Dispatch;

public class DeliveryChallanDetailDto
{
    public int Id { get; set; }
    public string DCNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int WorkOrderId { get; set; }
    public string WONumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerAddress { get; set; }
    public string? CustomerGSTIN { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? LRNumber { get; set; }
    public decimal? MaterialValueApprox { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<DCLineItemDto> Lines { get; set; } = [];
    public List<string> LoadingPhotoUrls { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class DCLineItemDto
{
    public int Id { get; set; }
    public int SectionProfileId { get; set; }
    public string SectionNumber { get; set; } = string.Empty;
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public string? CustomerDCRef { get; set; }
    public string? Remarks { get; set; }
}

public class UpdateDCStatusDto
{
    public string Status { get; set; } = string.Empty;
}

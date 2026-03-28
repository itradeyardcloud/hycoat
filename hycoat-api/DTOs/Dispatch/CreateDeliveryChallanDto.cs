namespace HycoatApi.DTOs.Dispatch;

public class CreateDeliveryChallanDto
{
    public DateTime Date { get; set; }
    public int WorkOrderId { get; set; }
    public int CustomerId { get; set; }
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? LRNumber { get; set; }
    public decimal? MaterialValueApprox { get; set; }
    public string? Notes { get; set; }
    public List<CreateDCLineItemDto> Lines { get; set; } = [];
}

public class CreateDCLineItemDto
{
    public int SectionProfileId { get; set; }
    public decimal LengthMM { get; set; }
    public int Quantity { get; set; }
    public string? CustomerDCRef { get; set; }
    public string? Remarks { get; set; }
}

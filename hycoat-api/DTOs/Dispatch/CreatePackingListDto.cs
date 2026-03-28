namespace HycoatApi.DTOs.Dispatch;

public class CreatePackingListDto
{
    public DateTime Date { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public int WorkOrderId { get; set; }
    public string? PackingType { get; set; }
    public int? BundleCount { get; set; }
    public string? Notes { get; set; }
    public List<CreatePackingListLineDto> Lines { get; set; } = [];
}

public class CreatePackingListLineDto
{
    public int SectionProfileId { get; set; }
    public int Quantity { get; set; }
    public decimal LengthMM { get; set; }
    public int? BundleNumber { get; set; }
    public string? Remarks { get; set; }
}

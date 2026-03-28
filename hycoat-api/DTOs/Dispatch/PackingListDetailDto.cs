namespace HycoatApi.DTOs.Dispatch;

public class PackingListDetailDto
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
    public string? Notes { get; set; }
    public List<PackingListLineDto> Lines { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class PackingListLineDto
{
    public int Id { get; set; }
    public int SectionProfileId { get; set; }
    public string SectionNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal LengthMM { get; set; }
    public int? BundleNumber { get; set; }
    public string? Remarks { get; set; }
}

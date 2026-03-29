namespace HycoatApi.DTOs.Sales;

public class WorkOrderTimelineDto
{
    public int WorkOrderId { get; set; }
    public string WONumber { get; set; } = string.Empty;
    public List<TimelineEventDto> Events { get; set; } = [];
}

public class TimelineEventDto
{
    public string Stage { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public DateTime? Date { get; set; }
    public string? PerformedBy { get; set; }
    public bool IsComplete { get; set; }
}

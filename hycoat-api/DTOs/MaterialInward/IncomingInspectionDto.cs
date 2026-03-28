namespace HycoatApi.DTOs.MaterialInward;

public class IncomingInspectionDto
{
    public int Id { get; set; }
    public string InspectionNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int MaterialInwardId { get; set; }
    public string InwardNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string OverallStatus { get; set; } = string.Empty;
    public string? InspectedByName { get; set; }
}

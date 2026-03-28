namespace HycoatApi.DTOs.Sales;

public class InquiryDto
{
    public int Id { get; set; }
    public string InquiryNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? ProjectName { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? ProcessTypeName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AssignedToName { get; set; }
}

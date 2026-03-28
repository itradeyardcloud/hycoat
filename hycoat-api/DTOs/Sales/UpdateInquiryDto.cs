namespace HycoatApi.DTOs.Sales;

public class UpdateInquiryDto
{
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public string? ProjectName { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public int? ProcessTypeId { get; set; }
    public string? Description { get; set; }
    public string? AssignedToUserId { get; set; }
}

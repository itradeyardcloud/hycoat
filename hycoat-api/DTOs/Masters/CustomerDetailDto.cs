namespace HycoatApi.DTOs.Masters;

public class CustomerDetailDto : CustomerDto
{
    public string? Address { get; set; }
    public string? Pincode { get; set; }
    public string? Notes { get; set; }
    public int InquiryCount { get; set; }
    public int WorkOrderCount { get; set; }
}

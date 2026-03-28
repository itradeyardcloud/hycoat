namespace HycoatApi.DTOs.Masters;

public class UpdateCustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? GSTIN { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
}

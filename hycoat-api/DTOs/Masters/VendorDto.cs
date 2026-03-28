namespace HycoatApi.DTOs.Masters;

public class VendorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string VendorType { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? State { get; set; }
    public string? GSTIN { get; set; }
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

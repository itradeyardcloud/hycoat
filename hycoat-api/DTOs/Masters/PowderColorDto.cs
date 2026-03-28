namespace HycoatApi.DTOs.Masters;

public class PowderColorDto
{
    public int Id { get; set; }
    public string PowderCode { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string? RALCode { get; set; }
    public string? Make { get; set; }
    public int? VendorId { get; set; }
    public string? VendorName { get; set; }
    public int? WarrantyYears { get; set; }
    public DateTime CreatedAt { get; set; }
}

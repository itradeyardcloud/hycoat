namespace HycoatApi.DTOs.Masters;

public class UpdatePowderColorDto
{
    public string PowderCode { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string? RALCode { get; set; }
    public string? Make { get; set; }
    public int? VendorId { get; set; }
    public int? WarrantyYears { get; set; }
    public string? Notes { get; set; }
}

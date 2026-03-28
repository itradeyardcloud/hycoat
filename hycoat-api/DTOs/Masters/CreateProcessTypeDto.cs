namespace HycoatApi.DTOs.Masters;

public class CreateProcessTypeDto
{
    public string Name { get; set; } = string.Empty;
    public decimal? DefaultRatePerSFT { get; set; }
    public string? Description { get; set; }
}

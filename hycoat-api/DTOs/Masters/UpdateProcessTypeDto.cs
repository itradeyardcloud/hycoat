namespace HycoatApi.DTOs.Masters;

public class UpdateProcessTypeDto
{
    public string Name { get; set; } = string.Empty;
    public decimal? DefaultRatePerSFT { get; set; }
    public string? Description { get; set; }
}

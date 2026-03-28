namespace HycoatApi.DTOs.Masters;

public class ProcessTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? DefaultRatePerSFT { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

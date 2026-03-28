namespace HycoatApi.DTOs.Masters;

public class UpdateSectionProfileDto
{
    public string SectionNumber { get; set; } = string.Empty;
    public string? Type { get; set; }
    public decimal PerimeterMM { get; set; }
    public decimal? WeightPerMeter { get; set; }
    public decimal? HeightMM { get; set; }
    public decimal? WidthMM { get; set; }
    public decimal? ThicknessMM { get; set; }
    public string? Notes { get; set; }
}

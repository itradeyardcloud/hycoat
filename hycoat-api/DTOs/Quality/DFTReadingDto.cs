namespace HycoatApi.DTOs.Quality;

public class DFTReadingDto
{
    public int Id { get; set; }
    public int? SectionProfileId { get; set; }
    public string? SectionProfileName { get; set; }
    public decimal? S1 { get; set; }
    public decimal? S2 { get; set; }
    public decimal? S3 { get; set; }
    public decimal? S4 { get; set; }
    public decimal? S5 { get; set; }
    public decimal? S6 { get; set; }
    public decimal? S7 { get; set; }
    public decimal? S8 { get; set; }
    public decimal? S9 { get; set; }
    public decimal? S10 { get; set; }
    public decimal? MinReading { get; set; }
    public decimal? MaxReading { get; set; }
    public decimal? AvgReading { get; set; }
    public bool IsWithinSpec { get; set; }
}

using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Quality;

public class InProcessDFTReading
{
    public int Id { get; set; }
    public int InProcessInspectionId { get; set; }
    public int? SectionProfileId { get; set; }
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

    // Navigation
    public InProcessInspection InProcessInspection { get; set; } = null!;
    public SectionProfile? SectionProfile { get; set; }
}

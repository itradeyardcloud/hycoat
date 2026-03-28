namespace HycoatApi.DTOs.Quality;

public class DFTTrendDto
{
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public decimal? AvgReading { get; set; }
    public decimal? MinReading { get; set; }
    public decimal? MaxReading { get; set; }
    public bool IsWithinSpec { get; set; }
}

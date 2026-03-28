namespace HycoatApi.DTOs.Quality;

public class TestResultDto
{
    public int Id { get; set; }
    public string TestType { get; set; } = string.Empty;
    public string? InstrumentName { get; set; }
    public string? InstrumentMake { get; set; }
    public string? InstrumentModel { get; set; }
    public DateTime? CalibrationDate { get; set; }
    public string? ReferenceStandard { get; set; }
    public string? StandardLimit { get; set; }
    public string Result { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

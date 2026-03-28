namespace HycoatApi.DTOs.Quality;

public class CreateInProcessInspectionDto
{
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public string? Remarks { get; set; }
    public List<CreateDFTReadingDto> DFTReadings { get; set; } = [];
    public List<CreateTestResultDto> TestResults { get; set; } = [];
}

public class CreateDFTReadingDto
{
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
}

public class CreateTestResultDto
{
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

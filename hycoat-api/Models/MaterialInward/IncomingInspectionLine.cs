namespace HycoatApi.Models.MaterialInward;

public class IncomingInspectionLine
{
    public int Id { get; set; }
    public int IncomingInspectionId { get; set; }
    public int MaterialInwardLineId { get; set; }
    public bool? WatermarkOk { get; set; }
    public bool? ScratchOk { get; set; }
    public bool? DentOk { get; set; }
    public bool? DimensionalCheckOk { get; set; }
    public bool BuffingRequired { get; set; }
    public decimal? BuffingCharge { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }

    // Navigation
    public IncomingInspection IncomingInspection { get; set; } = null!;
    public MaterialInwardLine MaterialInwardLine { get; set; } = null!;
}

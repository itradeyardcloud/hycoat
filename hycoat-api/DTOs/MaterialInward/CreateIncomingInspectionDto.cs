namespace HycoatApi.DTOs.MaterialInward;

public class CreateIncomingInspectionDto
{
    public DateTime Date { get; set; }
    public int MaterialInwardId { get; set; }
    public string? Remarks { get; set; }
    public List<CreateIncomingInspectionLineDto> Lines { get; set; } = new();
}

public class CreateIncomingInspectionLineDto
{
    public int MaterialInwardLineId { get; set; }
    public bool? WatermarkOk { get; set; }
    public bool? ScratchOk { get; set; }
    public bool? DentOk { get; set; }
    public bool? DimensionalCheckOk { get; set; }
    public bool BuffingRequired { get; set; }
    public decimal? BuffingCharge { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

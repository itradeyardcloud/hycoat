namespace HycoatApi.DTOs.MaterialInward;

public class CreateMaterialInwardDto
{
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public int? WorkOrderId { get; set; }
    public string? CustomerDCNumber { get; set; }
    public DateTime? CustomerDCDate { get; set; }
    public string? VehicleNumber { get; set; }
    public string? UnloadingLocation { get; set; }
    public int? ProcessTypeId { get; set; }
    public int? PowderColorId { get; set; }
    public string? Notes { get; set; }
    public List<CreateMaterialInwardLineDto> Lines { get; set; } = new();
}

public class CreateMaterialInwardLineDto
{
    public int SectionProfileId { get; set; }
    public decimal LengthMM { get; set; }
    public int QtyAsPerDC { get; set; }
    public int QtyReceived { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Remarks { get; set; }
}

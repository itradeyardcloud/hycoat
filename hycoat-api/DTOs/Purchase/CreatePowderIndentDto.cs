namespace HycoatApi.DTOs.Purchase;

public class CreatePowderIndentDto
{
    public DateTime Date { get; set; }
    public int? ProductionWorkOrderId { get; set; }
    public string? Notes { get; set; }
    public List<CreatePowderIndentLineDto> Lines { get; set; } = [];
}

public class CreatePowderIndentLineDto
{
    public int PowderColorId { get; set; }
    public decimal RequiredQtyKg { get; set; }
}

public class UpdatePowderIndentStatusDto
{
    public string Status { get; set; } = string.Empty;
}

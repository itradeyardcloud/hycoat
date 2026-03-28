namespace HycoatApi.DTOs.MaterialInward;

public class WorkOrderLookupDto
{
    public int Id { get; set; }
    public string WONumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int ProcessTypeId { get; set; }
    public string ProcessTypeName { get; set; } = string.Empty;
    public int? PowderColorId { get; set; }
    public string? PowderColorName { get; set; }
}

namespace HycoatApi.DTOs.Planning;

public class ProductionWorkOrderDetailDto : ProductionWorkOrderDto
{
    public int WorkOrderId { get; set; }
    public int CustomerId { get; set; }
    public int ProcessTypeId { get; set; }
    public string ProcessTypeName { get; set; } = string.Empty;
    public int? PowderColorId { get; set; }
    public int ProductionUnitId { get; set; }
    public decimal? PreTreatmentTimeHrs { get; set; }
    public decimal? PostTreatmentTimeHrs { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string? PackingType { get; set; }
    public string? SpecialInstructions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<PWOLineItemDetailDto> LineItems { get; set; } = new();
}

namespace HycoatApi.DTOs.Planning;

public class UpdateProductionWorkOrderDto
{
    public DateTime Date { get; set; }
    public int WorkOrderId { get; set; }
    public int ProcessTypeId { get; set; }
    public int? PowderColorId { get; set; }
    public int ProductionUnitId { get; set; }
    public string? ShiftAllocation { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DispatchDate { get; set; }
    public string? PackingType { get; set; }
    public string? SpecialInstructions { get; set; }
    public List<CreatePWOLineItemDto> LineItems { get; set; } = new();
}

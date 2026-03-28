namespace HycoatApi.DTOs.Planning;

public class CreateProductionWorkOrderDto
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

public class CreatePWOLineItemDto
{
    public int SectionProfileId { get; set; }
    public string? CustomerDCNo { get; set; }
    public int Quantity { get; set; }
    public decimal LengthMM { get; set; }
    public string? SpecialInstructions { get; set; }
}

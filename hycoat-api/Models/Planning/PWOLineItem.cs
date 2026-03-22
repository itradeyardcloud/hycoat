using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Planning;

public class PWOLineItem
{
    public int Id { get; set; }
    public int ProductionWorkOrderId { get; set; }
    public int SectionProfileId { get; set; }
    public string? CustomerDCNo { get; set; }
    public int Quantity { get; set; }
    public decimal LengthMM { get; set; }
    public decimal PerimeterMM { get; set; }
    public decimal UnitSurfaceAreaSqMtr { get; set; }
    public decimal TotalSurfaceAreaSqft { get; set; }
    public string? SpecialInstructions { get; set; }

    // Navigation
    public ProductionWorkOrder ProductionWorkOrder { get; set; } = null!;
    public SectionProfile SectionProfile { get; set; } = null!;
}

namespace HycoatApi.DTOs.Planning;

public class PWOLineItemDetailDto
{
    public int Id { get; set; }
    public int SectionProfileId { get; set; }
    public string SectionNumber { get; set; } = string.Empty;
    public string? CustomerDCNo { get; set; }
    public int Quantity { get; set; }
    public decimal LengthMM { get; set; }
    public decimal PerimeterMM { get; set; }
    public decimal UnitSurfaceAreaSqMtr { get; set; }
    public decimal TotalSurfaceAreaSqft { get; set; }
    public string? SpecialInstructions { get; set; }
    public ProductionTimeCalcDto? TimeCalc { get; set; }
}

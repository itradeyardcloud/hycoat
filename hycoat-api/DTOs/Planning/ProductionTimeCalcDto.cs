namespace HycoatApi.DTOs.Planning;

public class ProductionTimeCalcDto
{
    public decimal? ThicknessMM { get; set; }
    public decimal? WidthMM { get; set; }
    public decimal? HeightMM { get; set; }
    public decimal? SpecificWeight { get; set; }
    public decimal? WeightPerMtr { get; set; }
    public decimal? TotalWeightKg { get; set; }
    public int? LoadsRequired { get; set; }
    public decimal? TotalTimePreTreatMins { get; set; }
    public decimal? ConveyorSpeedMtrPerMin { get; set; }
    public decimal? JigLengthMM { get; set; }
    public decimal? GapBetweenJigsMM { get; set; }
    public decimal? TotalConveyorDistanceMtrs { get; set; }
    public decimal? TotalTimePostTreatMins { get; set; }
}

public class ProductionTimeCalcRequestDto
{
    public int ProductionUnitId { get; set; }
    public List<TimeCalcLineItemDto> LineItems { get; set; } = new();
}

public class TimeCalcLineItemDto
{
    public int SectionProfileId { get; set; }
    public int Quantity { get; set; }
    public decimal LengthMM { get; set; }
}

public class ProductionTimeCalcResultDto
{
    public decimal PreTreatmentTimeHrs { get; set; }
    public decimal PostTreatmentTimeHrs { get; set; }
    public decimal TotalTimeHrs { get; set; }
    public List<LineTimeCalcResultDto> Lines { get; set; } = new();
}

public class LineTimeCalcResultDto
{
    public int SectionProfileId { get; set; }
    public ProductionTimeCalcDto Calc { get; set; } = new();
}

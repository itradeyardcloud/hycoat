namespace HycoatApi.Models.Planning;

public class ProductionTimeCalc
{
    public int Id { get; set; }
    public int PWOLineItemId { get; set; }
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

    // Navigation
    public PWOLineItem PWOLineItem { get; set; } = null!;
}

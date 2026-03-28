using HycoatApi.DTOs.Planning;
using HycoatApi.Models.Masters;

namespace HycoatApi.Services.Planning;

public class ProductionTimeCalcService : IProductionTimeCalcService
{
    // Configurable constants
    private const decimal AluminumSpecificWeight = 2.71m;
    private const decimal TimePerLoadMins = 25m;
    private const decimal GapBetweenJigsMM = 500m;
    private const decimal ConveyorSpeedThin = 1.2m;    // < 1.5mm
    private const decimal ConveyorSpeedMedium = 1.0m;   // 1.5–2.5mm
    private const decimal ConveyorSpeedThick = 0.8m;    // > 2.5mm
    private const decimal ProfileGapMM = 50m;           // gap between profiles in basket

    public ProductionTimeCalcResultDto Calculate(
        List<TimeCalcLineItemDto> lineItems,
        List<SectionProfile> sectionProfiles,
        ProductionUnit productionUnit)
    {
        var result = new ProductionTimeCalcResultDto();
        decimal totalPreTreatMins = 0;
        decimal totalPostTreatMins = 0;

        foreach (var line in lineItems)
        {
            var section = sectionProfiles.FirstOrDefault(s => s.Id == line.SectionProfileId);
            if (section == null) continue;

            var calc = CalculateLine(line, section, productionUnit);
            result.Lines.Add(new LineTimeCalcResultDto
            {
                SectionProfileId = line.SectionProfileId,
                Calc = calc
            });

            totalPreTreatMins += calc.TotalTimePreTreatMins ?? 0;
            totalPostTreatMins += calc.TotalTimePostTreatMins ?? 0;
        }

        result.PreTreatmentTimeHrs = Math.Round(totalPreTreatMins / 60m, 2);
        result.PostTreatmentTimeHrs = Math.Round(totalPostTreatMins / 60m, 2);
        result.TotalTimeHrs = Math.Round((totalPreTreatMins + totalPostTreatMins) / 60m, 2);

        return result;
    }

    private ProductionTimeCalcDto CalculateLine(
        TimeCalcLineItemDto line,
        SectionProfile section,
        ProductionUnit unit)
    {
        var thickness = section.ThicknessMM ?? 0;
        var width = section.WidthMM ?? 0;
        var height = section.HeightMM ?? 0;
        var perimeter = section.PerimeterMM;

        // Weight calculation
        var specificWeight = AluminumSpecificWeight;
        var weightPerMtr = perimeter * thickness * specificWeight / 1_000_000m;
        var totalWeightKg = weightPerMtr * (line.LengthMM / 1000m) * line.Quantity;

        // Pre-treatment: basket capacity
        int loadsRequired = 1;
        if (unit.BucketLengthMM.HasValue && unit.BucketWidthMM.HasValue &&
            unit.BucketLengthMM.Value > 0 && line.LengthMM > 0)
        {
            var profilesPerRow = (int)Math.Floor(unit.BucketLengthMM.Value / (line.LengthMM + ProfileGapMM));
            if (profilesPerRow < 1) profilesPerRow = 1;

            var effectiveWidth = width > 0 ? width : (thickness > 0 ? thickness * 2 : 50);
            var profilesPerColumn = (int)Math.Floor(unit.BucketWidthMM.Value / (effectiveWidth + ProfileGapMM));
            if (profilesPerColumn < 1) profilesPerColumn = 1;

            var profilesPerBasket = profilesPerRow * profilesPerColumn;
            if (profilesPerBasket < 1) profilesPerBasket = 1;

            loadsRequired = (int)Math.Ceiling((decimal)line.Quantity / profilesPerBasket);
        }
        if (loadsRequired < 1) loadsRequired = 1;

        var totalTimePreTreatMins = loadsRequired * TimePerLoadMins;

        // Post-treatment: conveyor calculation
        var conveyorSpeed = thickness switch
        {
            < 1.5m => ConveyorSpeedThin,
            <= 2.5m => ConveyorSpeedMedium,
            _ => ConveyorSpeedThick
        };

        var jigLengthMM = line.LengthMM;
        var totalConveyorDistanceMtrs = line.Quantity * (jigLengthMM + GapBetweenJigsMM) / 1000m;
        var totalTimePostTreatMins = conveyorSpeed > 0
            ? totalConveyorDistanceMtrs / conveyorSpeed
            : 0;

        return new ProductionTimeCalcDto
        {
            ThicknessMM = thickness,
            WidthMM = width,
            HeightMM = height,
            SpecificWeight = specificWeight,
            WeightPerMtr = Math.Round(weightPerMtr, 4),
            TotalWeightKg = Math.Round(totalWeightKg, 2),
            LoadsRequired = loadsRequired,
            TotalTimePreTreatMins = Math.Round(totalTimePreTreatMins, 2),
            ConveyorSpeedMtrPerMin = conveyorSpeed,
            JigLengthMM = jigLengthMM,
            GapBetweenJigsMM = GapBetweenJigsMM,
            TotalConveyorDistanceMtrs = Math.Round(totalConveyorDistanceMtrs, 2),
            TotalTimePostTreatMins = Math.Round(totalTimePostTreatMins, 2)
        };
    }
}

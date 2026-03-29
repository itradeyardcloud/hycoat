namespace HycoatApi.DTOs.Reports;

public class CreateYieldReportDto
{
    public DateTime Date { get; set; }
    public string Shift { get; set; } = string.Empty;
    public int ProductionUnitId { get; set; }

    public decimal ProductionSFT { get; set; }
    public decimal RejectionSFT { get; set; }

    public decimal ElectricityOpeningKwh { get; set; }
    public decimal ElectricityClosingKwh { get; set; }
    public decimal ElectricityRatePerKwh { get; set; }

    public decimal OvenGasOpeningReading { get; set; }
    public decimal OvenGasClosingReading { get; set; }
    public decimal OvenGasRatePerUnit { get; set; }

    public decimal PowderUsedKg { get; set; }
    public decimal PowderRatePerKg { get; set; }

    public decimal ManpowerCost { get; set; }
    public decimal OtherCost { get; set; }
    public decimal SellingPricePerSFT { get; set; }

    public string? Remarks { get; set; }
}

public class YieldReportDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Shift { get; set; } = string.Empty;
    public int ProductionUnitId { get; set; }
    public string ProductionUnitName { get; set; } = string.Empty;

    public decimal ProductionSFT { get; set; }
    public decimal RejectionSFT { get; set; }
    public decimal NetProductionSFT { get; set; }
    public decimal YieldPercent { get; set; }

    public decimal ElectricityOpeningKwh { get; set; }
    public decimal ElectricityClosingKwh { get; set; }
    public decimal ElectricityConsumedKwh { get; set; }
    public decimal ElectricityRatePerKwh { get; set; }
    public decimal ElectricityCost { get; set; }

    public decimal OvenGasOpeningReading { get; set; }
    public decimal OvenGasClosingReading { get; set; }
    public decimal OvenGasConsumedUnits { get; set; }
    public decimal OvenGasRatePerUnit { get; set; }
    public decimal OvenGasCost { get; set; }

    public decimal PowderUsedKg { get; set; }
    public decimal PowderRatePerKg { get; set; }
    public decimal PowderCost { get; set; }

    public decimal ManpowerCost { get; set; }
    public decimal OtherCost { get; set; }

    public decimal TotalCost { get; set; }
    public decimal CostPerSFT { get; set; }

    public decimal SellingPricePerSFT { get; set; }
    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
    public decimal RoiPercent { get; set; }

    public string? Remarks { get; set; }
}

public class YieldSummaryDto
{
    public DateTime Date { get; set; }
    public decimal TotalProductionSFT { get; set; }
    public decimal TotalRejectionSFT { get; set; }
    public decimal TotalNetProductionSFT { get; set; }
    public decimal YieldPercent { get; set; }

    public decimal TotalElectricityConsumedKwh { get; set; }
    public decimal TotalGasConsumedUnits { get; set; }

    public decimal ElectricityCost { get; set; }
    public decimal OvenGasCost { get; set; }
    public decimal PowderCost { get; set; }
    public decimal ManpowerCost { get; set; }
    public decimal OtherCost { get; set; }

    public decimal TotalCost { get; set; }
    public decimal CostPerSFT { get; set; }
    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
    public decimal RoiPercent { get; set; }

    public int EntryCount { get; set; }
}
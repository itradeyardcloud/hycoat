using HycoatApi.Models.Common;
using HycoatApi.Models.Masters;

namespace HycoatApi.Models.Production;

public class YieldReport : BaseEntity
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

    public ProductionUnit ProductionUnit { get; set; } = null!;
}
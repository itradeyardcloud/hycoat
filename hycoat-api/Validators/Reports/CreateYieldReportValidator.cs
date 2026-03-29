using FluentValidation;
using HycoatApi.DTOs.Reports;

namespace HycoatApi.Validators.Reports;

public class CreateYieldReportValidator : AbstractValidator<CreateYieldReportDto>
{
    private static readonly string[] ValidShifts = ["Day", "Night"];

    public CreateYieldReportValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required");

        RuleFor(x => x.Shift)
            .NotEmpty().WithMessage("Shift is required")
            .Must(s => ValidShifts.Contains(s)).WithMessage("Shift must be Day or Night");

        RuleFor(x => x.ProductionUnitId)
            .GreaterThan(0).WithMessage("Production Unit is required");

        RuleFor(x => x.ProductionSFT)
            .GreaterThan(0).WithMessage("Production (SFT) must be greater than 0");

        RuleFor(x => x.RejectionSFT)
            .GreaterThanOrEqualTo(0).WithMessage("Rejection (SFT) cannot be negative")
            .LessThanOrEqualTo(x => x.ProductionSFT)
            .WithMessage("Rejection (SFT) cannot be greater than Production (SFT)");

        RuleFor(x => x.ElectricityOpeningKwh)
            .GreaterThanOrEqualTo(0).WithMessage("Electricity opening reading cannot be negative");
        RuleFor(x => x.ElectricityClosingKwh)
            .GreaterThanOrEqualTo(x => x.ElectricityOpeningKwh)
            .WithMessage("Electricity closing reading must be >= opening reading");
        RuleFor(x => x.ElectricityRatePerKwh)
            .GreaterThanOrEqualTo(0).WithMessage("Electricity rate cannot be negative");

        RuleFor(x => x.OvenGasOpeningReading)
            .GreaterThanOrEqualTo(0).WithMessage("Gas opening reading cannot be negative");
        RuleFor(x => x.OvenGasClosingReading)
            .GreaterThanOrEqualTo(x => x.OvenGasOpeningReading)
            .WithMessage("Gas closing reading must be >= opening reading");
        RuleFor(x => x.OvenGasRatePerUnit)
            .GreaterThanOrEqualTo(0).WithMessage("Gas rate cannot be negative");

        RuleFor(x => x.PowderUsedKg)
            .GreaterThanOrEqualTo(0).WithMessage("Powder used cannot be negative");
        RuleFor(x => x.PowderRatePerKg)
            .GreaterThanOrEqualTo(0).WithMessage("Powder rate cannot be negative");

        RuleFor(x => x.ManpowerCost)
            .GreaterThanOrEqualTo(0).WithMessage("Manpower cost cannot be negative");
        RuleFor(x => x.OtherCost)
            .GreaterThanOrEqualTo(0).WithMessage("Other cost cannot be negative");

        RuleFor(x => x.SellingPricePerSFT)
            .GreaterThanOrEqualTo(0).WithMessage("Selling price/SFT cannot be negative");

        RuleFor(x => x.Remarks)
            .MaximumLength(1000);
    }
}
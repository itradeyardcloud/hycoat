using FluentValidation;
using HycoatApi.DTOs.Production;

namespace HycoatApi.Validators.Production;

public class CreateProductionLogValidator : AbstractValidator<CreateProductionLogDto>
{
    private static readonly string[] ValidShifts = ["Day", "Night"];

    public CreateProductionLogValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required");
        RuleFor(x => x.Shift)
            .NotEmpty().WithMessage("Shift is required")
            .Must(s => ValidShifts.Contains(s)).WithMessage("Shift must be Day or Night");
        RuleFor(x => x.ProductionWorkOrderId).GreaterThan(0).WithMessage("Production Work Order is required");
        RuleFor(x => x.ConveyorSpeedMtrPerMin)
            .InclusiveBetween(0.1m, 5.0m).When(x => x.ConveyorSpeedMtrPerMin.HasValue)
            .WithMessage("Conveyor Speed must be between 0.1 and 5.0 m/min");
        RuleFor(x => x.OvenTemperature)
            .InclusiveBetween(150m, 300m).When(x => x.OvenTemperature.HasValue)
            .WithMessage("Oven Temperature must be between 150 and 300 °C");
        RuleFor(x => x.PowderBatchNo).MaximumLength(50);
        RuleFor(x => x.Remarks).MaximumLength(1000);
    }
}

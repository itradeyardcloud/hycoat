using FluentValidation;
using HycoatApi.DTOs.Planning;

namespace HycoatApi.Validators.Planning;

public class ReorderScheduleValidator : AbstractValidator<ReorderScheduleDto>
{
    private static readonly string[] ValidShifts = ["Day", "Night"];

    public ReorderScheduleValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required");
        RuleFor(x => x.Shift).NotEmpty().WithMessage("Shift is required")
            .Must(s => ValidShifts.Contains(s)).WithMessage("Shift must be Day or Night");
        RuleFor(x => x.ProductionUnitId).GreaterThan(0).WithMessage("Production Unit is required");
        RuleFor(x => x.ScheduleIds).NotEmpty().WithMessage("At least one schedule entry is required");
    }
}

using FluentValidation;
using HycoatApi.DTOs.Planning;

namespace HycoatApi.Validators.Planning;

public class CreateScheduleEntryValidator : AbstractValidator<CreateScheduleEntryDto>
{
    private static readonly string[] ValidShifts = ["Day", "Night"];

    public CreateScheduleEntryValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required");
        RuleFor(x => x.Shift).NotEmpty().WithMessage("Shift is required")
            .Must(s => ValidShifts.Contains(s)).WithMessage("Shift must be Day or Night");
        RuleFor(x => x.ProductionWorkOrderId).GreaterThan(0).WithMessage("Production Work Order is required");
        RuleFor(x => x.ProductionUnitId).GreaterThan(0).WithMessage("Production Unit is required");
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

using FluentValidation;
using HycoatApi.DTOs.Planning;

namespace HycoatApi.Validators.Planning;

public class UpdatePWOValidator : AbstractValidator<UpdateProductionWorkOrderDto>
{
    private static readonly string[] ValidShifts = ["Day", "Night", "Both"];

    public UpdatePWOValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required");
        RuleFor(x => x.WorkOrderId).GreaterThan(0).WithMessage("Work Order is required");
        RuleFor(x => x.ProcessTypeId).GreaterThan(0).WithMessage("Process Type is required");
        RuleFor(x => x.ProductionUnitId).GreaterThan(0).WithMessage("Production Unit is required");
        RuleFor(x => x.ShiftAllocation)
            .Must(s => string.IsNullOrEmpty(s) || ValidShifts.Contains(s))
            .WithMessage("Shift must be one of: Day, Night, Both");
        RuleFor(x => x.PackingType).MaximumLength(200);
        RuleFor(x => x.SpecialInstructions).MaximumLength(2000);
        RuleFor(x => x.LineItems).NotEmpty().WithMessage("At least one line item is required");
        RuleForEach(x => x.LineItems).SetValidator(new CreatePWOLineItemValidator());
    }
}

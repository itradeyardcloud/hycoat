using FluentValidation;
using HycoatApi.DTOs.MaterialInward;

namespace HycoatApi.Validators.MaterialInward;

public class CreateIncomingInspectionValidator : AbstractValidator<CreateIncomingInspectionDto>
{
    private static readonly string[] ValidStatuses = ["Pass", "Fail", "Conditional"];

    public CreateIncomingInspectionValidator()
    {
        RuleFor(x => x.MaterialInwardId).GreaterThan(0).WithMessage("Material Inward is required.");
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.Remarks).MaximumLength(2000);

        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one inspection line is required.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.MaterialInwardLineId).GreaterThan(0).WithMessage("Material Inward Line is required.");
            line.RuleFor(l => l.Status).NotEmpty().WithMessage("Status is required.")
                .Must(s => ValidStatuses.Contains(s)).WithMessage("Status must be Pass, Fail, or Conditional.");
            line.RuleFor(l => l.BuffingCharge).GreaterThanOrEqualTo(0)
                .When(l => l.BuffingCharge.HasValue).WithMessage("Buffing charge cannot be negative.");
            line.RuleFor(l => l.Remarks).MaximumLength(500);
        });
    }
}

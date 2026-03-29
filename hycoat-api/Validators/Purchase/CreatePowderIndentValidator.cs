using FluentValidation;
using HycoatApi.DTOs.Purchase;

namespace HycoatApi.Validators.Purchase;

public class CreatePowderIndentValidator : AbstractValidator<CreatePowderIndentDto>
{
    public CreatePowderIndentValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one powder line is required.");
        RuleForEach(x => x.Lines).SetValidator(new CreatePowderIndentLineValidator());
    }
}

public class CreatePowderIndentLineValidator : AbstractValidator<CreatePowderIndentLineDto>
{
    public CreatePowderIndentLineValidator()
    {
        RuleFor(x => x.PowderColorId).GreaterThan(0).WithMessage("Powder color is required.");
        RuleFor(x => x.RequiredQtyKg).GreaterThan(0).WithMessage("Required quantity must be greater than 0.");
    }
}

public class UpdatePowderIndentStatusValidator : AbstractValidator<UpdatePowderIndentStatusDto>
{
    private static readonly string[] ValidStatuses = ["Requested", "Approved", "Ordered", "Received"];

    public UpdatePowderIndentStatusValidator()
    {
        RuleFor(x => x.Status).NotEmpty().WithMessage("Status is required.")
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage("Status must be one of: Requested, Approved, Ordered, Received.");
    }
}

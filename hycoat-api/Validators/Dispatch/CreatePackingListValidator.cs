using FluentValidation;
using HycoatApi.DTOs.Dispatch;

namespace HycoatApi.Validators.Dispatch;

public class CreatePackingListValidator : AbstractValidator<CreatePackingListDto>
{
    public CreatePackingListValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.ProductionWorkOrderId).GreaterThan(0).WithMessage("Production Work Order is required.");
        RuleFor(x => x.WorkOrderId).GreaterThan(0).WithMessage("Work Order is required.");
        RuleFor(x => x.PackingType).MaximumLength(100);
        RuleFor(x => x.BundleCount).GreaterThan(0).When(x => x.BundleCount.HasValue)
            .WithMessage("Bundle count must be greater than 0.");
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one packing line is required.");
        RuleForEach(x => x.Lines).SetValidator(new CreatePackingListLineValidator());
    }
}

public class CreatePackingListLineValidator : AbstractValidator<CreatePackingListLineDto>
{
    public CreatePackingListLineValidator()
    {
        RuleFor(x => x.SectionProfileId).GreaterThan(0).WithMessage("Section profile is required.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        RuleFor(x => x.LengthMM).GreaterThan(0).WithMessage("Length must be greater than 0.");
        RuleFor(x => x.Remarks).MaximumLength(500);
    }
}

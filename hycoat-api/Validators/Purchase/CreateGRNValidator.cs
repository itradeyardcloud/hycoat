using FluentValidation;
using HycoatApi.DTOs.Purchase;

namespace HycoatApi.Validators.Purchase;

public class CreateGRNValidator : AbstractValidator<CreateGRNDto>
{
    public CreateGRNValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.PurchaseOrderId).GreaterThan(0).WithMessage("Purchase Order is required.");
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line item is required.");
        RuleForEach(x => x.Lines).SetValidator(new CreateGRNLineItemValidator());
    }
}

public class CreateGRNLineItemValidator : AbstractValidator<CreateGRNLineItemDto>
{
    public CreateGRNLineItemValidator()
    {
        RuleFor(x => x.PowderColorId).GreaterThan(0).WithMessage("Powder color is required.");
        RuleFor(x => x.QtyReceivedKg).GreaterThan(0).WithMessage("Received quantity must be greater than 0.");
        RuleFor(x => x.BatchCode).MaximumLength(100);
    }
}

public class UpdateReorderLevelValidator : AbstractValidator<UpdateReorderLevelDto>
{
    public UpdateReorderLevelValidator()
    {
        RuleFor(x => x.ReorderLevelKg).GreaterThanOrEqualTo(0).WithMessage("Reorder level must be 0 or greater.");
    }
}

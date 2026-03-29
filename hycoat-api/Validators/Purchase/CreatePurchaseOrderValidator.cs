using FluentValidation;
using HycoatApi.DTOs.Purchase;

namespace HycoatApi.Validators.Purchase;

public class CreatePurchaseOrderValidator : AbstractValidator<CreatePurchaseOrderDto>
{
    public CreatePurchaseOrderValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.VendorId).GreaterThan(0).WithMessage("Vendor is required.");
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line item is required.");
        RuleForEach(x => x.Lines).SetValidator(new CreatePOLineItemValidator());
    }
}

public class CreatePOLineItemValidator : AbstractValidator<CreatePOLineItemDto>
{
    public CreatePOLineItemValidator()
    {
        RuleFor(x => x.PowderColorId).GreaterThan(0).WithMessage("Powder color is required.");
        RuleFor(x => x.QtyKg).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        RuleFor(x => x.RatePerKg).GreaterThan(0).WithMessage("Rate per kg must be greater than 0.");
    }
}

public class UpdatePurchaseOrderStatusValidator : AbstractValidator<UpdatePurchaseOrderStatusDto>
{
    private static readonly string[] ValidStatuses = ["Draft", "Sent", "PartiallyReceived", "Received", "Cancelled"];

    public UpdatePurchaseOrderStatusValidator()
    {
        RuleFor(x => x.Status).NotEmpty().WithMessage("Status is required.")
            .Must(s => ValidStatuses.Contains(s))
            .WithMessage("Status must be one of: Draft, Sent, PartiallyReceived, Received, Cancelled.");
    }
}

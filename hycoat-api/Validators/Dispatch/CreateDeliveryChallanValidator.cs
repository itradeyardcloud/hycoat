using FluentValidation;
using HycoatApi.DTOs.Dispatch;

namespace HycoatApi.Validators.Dispatch;

public class CreateDeliveryChallanValidator : AbstractValidator<CreateDeliveryChallanDto>
{
    public CreateDeliveryChallanValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.WorkOrderId).GreaterThan(0).WithMessage("Work Order is required.");
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Customer is required.");
        RuleFor(x => x.VehicleNumber).MaximumLength(20)
            .Matches(@"^[A-Za-z0-9\-\s]*$").When(x => !string.IsNullOrEmpty(x.VehicleNumber))
            .WithMessage("Vehicle number must contain only letters, numbers, hyphens, and spaces.");
        RuleFor(x => x.DriverName).MaximumLength(200);
        RuleFor(x => x.LRNumber).MaximumLength(50);
        RuleFor(x => x.MaterialValueApprox).GreaterThanOrEqualTo(0)
            .When(x => x.MaterialValueApprox.HasValue);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line item is required.");
        RuleForEach(x => x.Lines).SetValidator(new CreateDCLineItemValidator());
    }
}

public class CreateDCLineItemValidator : AbstractValidator<CreateDCLineItemDto>
{
    public CreateDCLineItemValidator()
    {
        RuleFor(x => x.SectionProfileId).GreaterThan(0).WithMessage("Section profile is required.");
        RuleFor(x => x.LengthMM).GreaterThan(0).WithMessage("Length must be greater than 0.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        RuleFor(x => x.CustomerDCRef).MaximumLength(100);
        RuleFor(x => x.Remarks).MaximumLength(500);
    }
}

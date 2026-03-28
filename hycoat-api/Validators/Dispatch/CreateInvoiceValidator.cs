using FluentValidation;
using HycoatApi.DTOs.Dispatch;

namespace HycoatApi.Validators.Dispatch;

public class CreateInvoiceValidator : AbstractValidator<CreateInvoiceDto>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Customer is required.");
        RuleFor(x => x.WorkOrderId).GreaterThan(0).WithMessage("Work Order is required.");
        RuleFor(x => x.HSNSACCode).MaximumLength(20);
        RuleFor(x => x.PackingCharges).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TransportCharges).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CGSTRate).InclusiveBetween(0, 28).WithMessage("CGST rate must be between 0 and 28%.");
        RuleFor(x => x.SGSTRate).InclusiveBetween(0, 28).WithMessage("SGST rate must be between 0 and 28%.");
        RuleFor(x => x.IGSTRate).InclusiveBetween(0, 28).WithMessage("IGST rate must be between 0 and 28%.");
        RuleFor(x => x.PaymentTerms).MaximumLength(500);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one line item is required.");
        RuleForEach(x => x.Lines).SetValidator(new CreateInvoiceLineItemValidator());
    }
}

public class CreateInvoiceLineItemValidator : AbstractValidator<CreateInvoiceLineItemDto>
{
    public CreateInvoiceLineItemValidator()
    {
        RuleFor(x => x.SectionProfileId).GreaterThan(0).WithMessage("Section profile is required.");
        RuleFor(x => x.PerimeterMM).GreaterThan(0).WithMessage("Perimeter must be greater than 0.");
        RuleFor(x => x.LengthMM).GreaterThan(0).WithMessage("Length must be greater than 0.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0.");
        RuleFor(x => x.RatePerSFT).GreaterThan(0).WithMessage("Rate per SFT must be greater than 0.");
        RuleFor(x => x.Color).MaximumLength(100);
        RuleFor(x => x.MicronRange).MaximumLength(50);
        RuleFor(x => x.DCNumber).MaximumLength(50);
    }
}

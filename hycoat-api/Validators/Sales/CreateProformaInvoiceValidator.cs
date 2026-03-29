using FluentValidation;
using HycoatApi.DTOs.Sales;

namespace HycoatApi.Validators.Sales;

public class CreateProformaInvoiceValidator : AbstractValidator<CreateProformaInvoiceDto>
{
    public CreateProformaInvoiceValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Customer is required");
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required");
        RuleFor(x => x.PackingCharges).GreaterThanOrEqualTo(0).WithMessage("Packing charges cannot be negative");
        RuleFor(x => x.TransportCharges).GreaterThanOrEqualTo(0).WithMessage("Transport charges cannot be negative");
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.LineItems).NotEmpty().WithMessage("At least one line item is required");
        RuleForEach(x => x.LineItems).ChildRules(line =>
        {
            line.RuleFor(x => x.SectionProfileId).GreaterThan(0).WithMessage("Section profile is required");
            line.RuleFor(x => x.LengthMM).GreaterThan(0).WithMessage("Length must be greater than 0");
            line.RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than 0");
            line.RuleFor(x => x.RatePerSFT).GreaterThan(0).WithMessage("Rate per SFT must be greater than 0");
        });
    }
}

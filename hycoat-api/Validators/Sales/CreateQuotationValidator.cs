using FluentValidation;
using HycoatApi.DTOs.Sales;

namespace HycoatApi.Validators.Sales;

public class CreateQuotationValidator : AbstractValidator<CreateQuotationDto>
{
    public CreateQuotationValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Customer is required");
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required");
        RuleFor(x => x.ValidityDays).InclusiveBetween(1, 365).WithMessage("Validity must be 1-365 days");
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.LineItems).NotEmpty().WithMessage("At least one line item is required");
        RuleForEach(x => x.LineItems).SetValidator(new CreateQuotationLineItemValidator());
    }
}

public class CreateQuotationLineItemValidator : AbstractValidator<CreateQuotationLineItemDto>
{
    public CreateQuotationLineItemValidator()
    {
        RuleFor(x => x.ProcessTypeId).GreaterThan(0).WithMessage("Process type is required");
        RuleFor(x => x.RatePerSFT).GreaterThan(0).WithMessage("Rate per SFT must be greater than 0");
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.MicronRange).MaximumLength(50);
        RuleFor(x => x.WarrantyYears).GreaterThanOrEqualTo(0).When(x => x.WarrantyYears.HasValue);
    }
}

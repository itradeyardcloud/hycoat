using FluentValidation;
using HycoatApi.DTOs.Sales;

namespace HycoatApi.Validators.Sales;

public class UpdateInquiryValidator : AbstractValidator<UpdateInquiryDto>
{
    private static readonly string[] ValidSources = ["Email", "Phone", "WhatsApp", "Walk-in", "Tender"];

    public UpdateInquiryValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Customer is required");
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required")
            .LessThanOrEqualTo(DateTime.UtcNow.Date.AddDays(1)).WithMessage("Date cannot be in the future");
        RuleFor(x => x.Source).NotEmpty().WithMessage("Source is required")
            .Must(s => ValidSources.Contains(s)).WithMessage("Source must be one of: Email, Phone, WhatsApp, Walk-in, Tender");
        RuleFor(x => x.ProjectName).MaximumLength(300);
        RuleFor(x => x.ContactPerson).MaximumLength(200);
        RuleFor(x => x.ContactEmail).MaximumLength(200)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.ContactEmail));
        RuleFor(x => x.ContactPhone).MaximumLength(20);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

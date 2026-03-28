using FluentValidation;
using HycoatApi.DTOs.Masters;

namespace HycoatApi.Validators.Masters;

public class CreateVendorValidator : AbstractValidator<CreateVendorDto>
{
    private static readonly string[] ValidVendorTypes = ["Powder", "Chemical", "Consumable", "Other"];

    public CreateVendorValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.VendorType).NotEmpty().MaximumLength(50)
            .Must(vt => ValidVendorTypes.Contains(vt))
            .WithMessage("VendorType must be one of: Powder, Chemical, Consumable, Other");
        RuleFor(x => x.Address).MaximumLength(500);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.State).MaximumLength(100);
        RuleFor(x => x.GSTIN).MaximumLength(15)
            .Matches(@"^\d{2}[A-Z]{5}\d{4}[A-Z]{1}[A-Z\d]{1}[Z]{1}[A-Z\d]{1}$")
            .When(x => !string.IsNullOrEmpty(x.GSTIN))
            .WithMessage("Invalid GSTIN format");
        RuleFor(x => x.ContactPerson).MaximumLength(200);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Email).MaximumLength(200)
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email));
    }
}

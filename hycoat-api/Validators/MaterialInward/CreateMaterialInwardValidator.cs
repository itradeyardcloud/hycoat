using FluentValidation;
using HycoatApi.DTOs.MaterialInward;

namespace HycoatApi.Validators.MaterialInward;

public class CreateMaterialInwardValidator : AbstractValidator<CreateMaterialInwardDto>
{
    public CreateMaterialInwardValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Customer is required.");
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.CustomerDCNumber).MaximumLength(100);
        RuleFor(x => x.VehicleNumber).MaximumLength(20);
        RuleFor(x => x.UnloadingLocation).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(2000);

        RuleFor(x => x.Lines).NotEmpty().WithMessage("At least one material line is required.");
        RuleForEach(x => x.Lines).SetValidator(new CreateMaterialInwardLineValidator());
    }
}

public class CreateMaterialInwardLineValidator : AbstractValidator<CreateMaterialInwardLineDto>
{
    public CreateMaterialInwardLineValidator()
    {
        RuleFor(x => x.SectionProfileId).GreaterThan(0).WithMessage("Section profile is required.");
        RuleFor(x => x.LengthMM).GreaterThan(0).WithMessage("Length must be greater than 0.");
        RuleFor(x => x.QtyAsPerDC).GreaterThan(0).WithMessage("DC quantity must be greater than 0.");
        RuleFor(x => x.QtyReceived).GreaterThanOrEqualTo(0).WithMessage("Received quantity cannot be negative.");
        RuleFor(x => x.WeightKg).GreaterThanOrEqualTo(0).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.Remarks).MaximumLength(500);
    }
}

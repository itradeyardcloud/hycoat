using FluentValidation;
using HycoatApi.DTOs.Quality;

namespace HycoatApi.Validators.Quality;

public class CreateTestCertificateValidator : AbstractValidator<CreateTestCertificateDto>
{
    private static readonly string[] ValidWarranties = ["15 Years", "25 Years"];

    public CreateTestCertificateValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.FinalInspectionId).GreaterThan(0).WithMessage("Final Inspection is required.");
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Customer is required.");
        RuleFor(x => x.WorkOrderId).GreaterThan(0).WithMessage("Work Order is required.");
        RuleFor(x => x.LotQuantity).GreaterThan(0).WithMessage("Lot quantity must be greater than 0.");

        RuleFor(x => x.Warranty)
            .Must(w => w == null || ValidWarranties.Contains(w))
            .WithMessage("Warranty must be '15 Years' or '25 Years'.");

        RuleFor(x => x.ProductCode).MaximumLength(50);
        RuleFor(x => x.ProjectName).MaximumLength(200);
        RuleFor(x => x.SubstrateResult).MaximumLength(200);
        RuleFor(x => x.BakingTempResult).MaximumLength(200);
        RuleFor(x => x.BakingTimeResult).MaximumLength(200);
        RuleFor(x => x.ColorResult).MaximumLength(200);
        RuleFor(x => x.DFTResult).MaximumLength(200);
        RuleFor(x => x.MEKResult).MaximumLength(200);
        RuleFor(x => x.CrossCutResult).MaximumLength(200);
        RuleFor(x => x.ConicalMandrelResult).MaximumLength(200);
        RuleFor(x => x.BoilingWaterResult).MaximumLength(200);
    }
}

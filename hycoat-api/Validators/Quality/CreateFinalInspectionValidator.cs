using FluentValidation;
using HycoatApi.DTOs.Quality;

namespace HycoatApi.Validators.Quality;

public class CreateFinalInspectionValidator : AbstractValidator<CreateFinalInspectionDto>
{
    private static readonly string[] ValidOverallStatuses = ["Approved", "Rejected", "Rework"];

    public CreateFinalInspectionValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.ProductionWorkOrderId).GreaterThan(0).WithMessage("Production Work Order is required.");
        RuleFor(x => x.LotQuantity).GreaterThan(0).WithMessage("Lot quantity must be greater than 0.");
        RuleFor(x => x.SampledQuantity).GreaterThan(0).WithMessage("Sampled quantity must be greater than 0.")
            .LessThanOrEqualTo(x => x.LotQuantity).WithMessage("Sampled quantity must be ≤ lot quantity.");

        RuleFor(x => x.VisualCheckStatus)
            .Must(s => s == null || s == "Pass" || s == "Fail")
            .WithMessage("Visual check status must be Pass or Fail.");
        RuleFor(x => x.DFTRecheckStatus)
            .Must(s => s == null || s == "Pass" || s == "Fail")
            .WithMessage("DFT re-check status must be Pass or Fail.");
        RuleFor(x => x.ShadeMatchFinalStatus)
            .Must(s => s == null || s == "Pass" || s == "Fail")
            .WithMessage("Shade match status must be Pass or Fail.");

        RuleFor(x => x.OverallStatus).NotEmpty().WithMessage("Overall status is required.")
            .Must(s => ValidOverallStatuses.Contains(s))
            .WithMessage("Overall status must be Approved, Rejected, or Rework.");

        RuleFor(x => x.Remarks).MaximumLength(2000);
    }
}

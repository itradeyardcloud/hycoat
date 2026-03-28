using FluentValidation;
using HycoatApi.DTOs.Quality;

namespace HycoatApi.Validators.Quality;

public class CreatePanelTestValidator : AbstractValidator<CreatePanelTestDto>
{
    public CreatePanelTestValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.ProductionWorkOrderId).GreaterThan(0).WithMessage("Production Work Order is required.");

        RuleFor(x => x.BoilingWaterStatus)
            .Must(s => s == null || s == "Pass" || s == "Fail")
            .WithMessage("Boiling water status must be Pass or Fail.");
        RuleFor(x => x.ImpactTestStatus)
            .Must(s => s == null || s == "Pass" || s == "Fail")
            .WithMessage("Impact test status must be Pass or Fail.");
        RuleFor(x => x.ConicalMandrelStatus)
            .Must(s => s == null || s == "Pass" || s == "Fail")
            .WithMessage("Conical mandrel status must be Pass or Fail.");
        RuleFor(x => x.PencilHardnessStatus)
            .Must(s => s == null || s == "Pass" || s == "Fail")
            .WithMessage("Pencil hardness status must be Pass or Fail.");

        RuleFor(x => x.BoilingWaterResult).MaximumLength(200);
        RuleFor(x => x.ImpactTestResult).MaximumLength(200);
        RuleFor(x => x.ConicalMandrelResult).MaximumLength(200);
        RuleFor(x => x.PencilHardnessResult).MaximumLength(200);
        RuleFor(x => x.Remarks).MaximumLength(2000);
    }
}

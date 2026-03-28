using FluentValidation;
using HycoatApi.DTOs.Quality;

namespace HycoatApi.Validators.Quality;

public class CreateInProcessInspectionValidator : AbstractValidator<CreateInProcessInspectionDto>
{
    public CreateInProcessInspectionValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required.");
        RuleFor(x => x.ProductionWorkOrderId).GreaterThan(0).WithMessage("Production Work Order is required.");
        RuleFor(x => x.Remarks).MaximumLength(2000);

        RuleForEach(x => x.DFTReadings).SetValidator(new CreateDFTReadingValidator());
        RuleForEach(x => x.TestResults).SetValidator(new CreateTestResultValidator());
    }
}

public class CreateDFTReadingValidator : AbstractValidator<CreateDFTReadingDto>
{
    public CreateDFTReadingValidator()
    {
        RuleFor(x => x.S1).InclusiveBetween(0, 200).When(x => x.S1.HasValue).WithMessage("S1 must be between 0 and 200 microns.");
        RuleFor(x => x.S2).InclusiveBetween(0, 200).When(x => x.S2.HasValue).WithMessage("S2 must be between 0 and 200 microns.");
        RuleFor(x => x.S3).InclusiveBetween(0, 200).When(x => x.S3.HasValue).WithMessage("S3 must be between 0 and 200 microns.");
        RuleFor(x => x.S4).InclusiveBetween(0, 200).When(x => x.S4.HasValue).WithMessage("S4 must be between 0 and 200 microns.");
        RuleFor(x => x.S5).InclusiveBetween(0, 200).When(x => x.S5.HasValue).WithMessage("S5 must be between 0 and 200 microns.");
        RuleFor(x => x.S6).InclusiveBetween(0, 200).When(x => x.S6.HasValue).WithMessage("S6 must be between 0 and 200 microns.");
        RuleFor(x => x.S7).InclusiveBetween(0, 200).When(x => x.S7.HasValue).WithMessage("S7 must be between 0 and 200 microns.");
        RuleFor(x => x.S8).InclusiveBetween(0, 200).When(x => x.S8.HasValue).WithMessage("S8 must be between 0 and 200 microns.");
        RuleFor(x => x.S9).InclusiveBetween(0, 200).When(x => x.S9.HasValue).WithMessage("S9 must be between 0 and 200 microns.");
        RuleFor(x => x.S10).InclusiveBetween(0, 200).When(x => x.S10.HasValue).WithMessage("S10 must be between 0 and 200 microns.");
    }
}

public class CreateTestResultValidator : AbstractValidator<CreateTestResultDto>
{
    private static readonly string[] ValidTestTypes =
        ["DryFilmThickness", "Polymerisation", "Adhesion", "ShadeMatch", "GlossLevel"];

    public CreateTestResultValidator()
    {
        RuleFor(x => x.TestType).NotEmpty().WithMessage("Test type is required.")
            .Must(t => ValidTestTypes.Contains(t)).WithMessage("Test type must be one of: DryFilmThickness, Polymerisation, Adhesion, ShadeMatch, GlossLevel.");
        RuleFor(x => x.Result).NotEmpty().WithMessage("Result is required.").MaximumLength(200);
        RuleFor(x => x.Status).NotEmpty().WithMessage("Status is required.")
            .Must(s => s == "Pass" || s == "Fail").WithMessage("Status must be Pass or Fail.");
        RuleFor(x => x.InstrumentName).MaximumLength(100);
        RuleFor(x => x.InstrumentMake).MaximumLength(100);
        RuleFor(x => x.InstrumentModel).MaximumLength(100);
        RuleFor(x => x.ReferenceStandard).MaximumLength(200);
        RuleFor(x => x.StandardLimit).MaximumLength(200);
        RuleFor(x => x.Remarks).MaximumLength(1000);
    }
}

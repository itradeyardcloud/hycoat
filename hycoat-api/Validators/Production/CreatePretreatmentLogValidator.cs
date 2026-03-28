using FluentValidation;
using HycoatApi.DTOs.Production;

namespace HycoatApi.Validators.Production;

public class CreatePretreatmentLogValidator : AbstractValidator<CreatePretreatmentLogDto>
{
    private static readonly string[] ValidShifts = ["Day", "Night"];

    private static readonly string[] ValidTankNames =
    [
        "Degreasing",
        "Water Rinse 1",
        "Etching",
        "Water Rinse 2",
        "Deoxidizing / De-smutting",
        "Water Rinse 3",
        "Chrome Conversion Coating",
        "Water Rinse 4",
        "DI Water Rinse",
        "Oven Dry"
    ];

    public CreatePretreatmentLogValidator()
    {
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required");
        RuleFor(x => x.Shift)
            .NotEmpty().WithMessage("Shift is required")
            .Must(s => ValidShifts.Contains(s)).WithMessage("Shift must be Day or Night");
        RuleFor(x => x.ProductionWorkOrderId).GreaterThan(0).WithMessage("Production Work Order is required");
        RuleFor(x => x.BasketNumber).GreaterThan(0).WithMessage("Basket Number must be greater than 0");
        RuleFor(x => x.EtchTimeMins)
            .GreaterThan(0).When(x => x.EtchTimeMins.HasValue)
            .WithMessage("Etch Time must be greater than 0");
        RuleFor(x => x.Remarks).MaximumLength(1000);
        RuleForEach(x => x.TankReadings).ChildRules(tank =>
        {
            tank.RuleFor(x => x.TankName)
                .NotEmpty().WithMessage("Tank Name is required")
                .Must(name => ValidTankNames.Contains(name))
                .WithMessage("Tank Name must be from the predefined list");
            tank.RuleFor(x => x.ChemicalAdded).MaximumLength(200);
        });
    }
}

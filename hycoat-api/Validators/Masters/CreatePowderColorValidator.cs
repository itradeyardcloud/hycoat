using FluentValidation;
using HycoatApi.DTOs.Masters;

namespace HycoatApi.Validators.Masters;

public class CreatePowderColorValidator : AbstractValidator<CreatePowderColorDto>
{
    public CreatePowderColorValidator()
    {
        RuleFor(x => x.PowderCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ColorName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RALCode).MaximumLength(20);
        RuleFor(x => x.Make).MaximumLength(100);
        RuleFor(x => x.Notes).MaximumLength(500);
    }
}

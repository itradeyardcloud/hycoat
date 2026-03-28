using FluentValidation;
using HycoatApi.Data;
using HycoatApi.DTOs.Masters;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Validators.Masters;

public class CreateProcessTypeValidator : AbstractValidator<CreateProcessTypeDto>
{
    public CreateProcessTypeValidator(AppDbContext db)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
            .MustAsync(async (name, ct) => !await db.ProcessTypes.AnyAsync(p => p.Name == name, ct))
            .WithMessage("Process type name already exists");
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DefaultRatePerSFT).GreaterThanOrEqualTo(0)
            .When(x => x.DefaultRatePerSFT.HasValue);
    }
}

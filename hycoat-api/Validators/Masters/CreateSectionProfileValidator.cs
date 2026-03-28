using FluentValidation;
using HycoatApi.Data;
using HycoatApi.DTOs.Masters;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Validators.Masters;

public class CreateSectionProfileValidator : AbstractValidator<CreateSectionProfileDto>
{
    public CreateSectionProfileValidator(AppDbContext db)
    {
        RuleFor(x => x.SectionNumber).NotEmpty().MaximumLength(50)
            .MustAsync(async (sn, ct) => !await db.SectionProfiles.AnyAsync(s => s.SectionNumber == sn, ct))
            .WithMessage("Section number already exists");
        RuleFor(x => x.Type).MaximumLength(100);
        RuleFor(x => x.PerimeterMM).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

using FluentValidation;
using HycoatApi.Data;
using HycoatApi.DTOs.Masters;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Validators.Masters;

public class CreateProductionUnitValidator : AbstractValidator<CreateProductionUnitDto>
{
    public CreateProductionUnitValidator(AppDbContext db)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50)
            .MustAsync(async (name, ct) => !await db.ProductionUnits.AnyAsync(p => p.Name == name, ct))
            .WithMessage("Production unit name already exists");
    }
}

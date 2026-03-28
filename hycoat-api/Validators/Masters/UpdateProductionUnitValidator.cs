using FluentValidation;
using HycoatApi.Data;
using HycoatApi.DTOs.Masters;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Validators.Masters;

public class UpdateProductionUnitValidator : AbstractValidator<UpdateProductionUnitDto>
{
    public UpdateProductionUnitValidator(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(50)
            .MustAsync(async (name, ct) =>
            {
                var routeId = httpContextAccessor.HttpContext?.Request.RouteValues["id"]?.ToString();
                if (int.TryParse(routeId, out var id))
                    return !await db.ProductionUnits.AnyAsync(p => p.Name == name && p.Id != id, ct);
                return true;
            })
            .WithMessage("Production unit name already exists");
    }
}

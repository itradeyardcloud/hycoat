using FluentValidation;
using HycoatApi.Data;
using HycoatApi.DTOs.Masters;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Validators.Masters;

public class UpdateProcessTypeValidator : AbstractValidator<UpdateProcessTypeDto>
{
    public UpdateProcessTypeValidator(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
            .MustAsync(async (name, ct) =>
            {
                var routeId = httpContextAccessor.HttpContext?.Request.RouteValues["id"]?.ToString();
                if (int.TryParse(routeId, out var id))
                    return !await db.ProcessTypes.AnyAsync(p => p.Name == name && p.Id != id, ct);
                return true;
            })
            .WithMessage("Process type name already exists");
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x.DefaultRatePerSFT).GreaterThanOrEqualTo(0)
            .When(x => x.DefaultRatePerSFT.HasValue);
    }
}

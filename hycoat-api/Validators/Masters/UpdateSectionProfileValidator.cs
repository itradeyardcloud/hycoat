using FluentValidation;
using HycoatApi.Data;
using HycoatApi.DTOs.Masters;
using Microsoft.EntityFrameworkCore;

namespace HycoatApi.Validators.Masters;

public class UpdateSectionProfileValidator : AbstractValidator<UpdateSectionProfileDto>
{
    public UpdateSectionProfileValidator(AppDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        RuleFor(x => x.SectionNumber).NotEmpty().MaximumLength(50)
            .MustAsync(async (sn, ct) =>
            {
                var routeId = httpContextAccessor.HttpContext?.Request.RouteValues["id"]?.ToString();
                if (int.TryParse(routeId, out var id))
                    return !await db.SectionProfiles.AnyAsync(s => s.SectionNumber == sn && s.Id != id, ct);
                return true;
            })
            .WithMessage("Section number already exists");
        RuleFor(x => x.Type).MaximumLength(100);
        RuleFor(x => x.PerimeterMM).GreaterThan(0);
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

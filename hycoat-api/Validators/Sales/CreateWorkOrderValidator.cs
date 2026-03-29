using FluentValidation;
using HycoatApi.DTOs.Sales;

namespace HycoatApi.Validators.Sales;

public class CreateWorkOrderValidator : AbstractValidator<CreateWorkOrderDto>
{
    public CreateWorkOrderValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Customer is required");
        RuleFor(x => x.ProcessTypeId).GreaterThan(0).WithMessage("Process type is required");
        RuleFor(x => x.Date).NotEmpty().WithMessage("Date is required");
        RuleFor(x => x.DispatchDate)
            .GreaterThanOrEqualTo(x => x.Date)
            .When(x => x.DispatchDate.HasValue)
            .WithMessage("Dispatch date must be on or after the work order date");
        RuleFor(x => x.ProjectName).MaximumLength(300);
        RuleFor(x => x.Notes).MaximumLength(2000);
    }
}

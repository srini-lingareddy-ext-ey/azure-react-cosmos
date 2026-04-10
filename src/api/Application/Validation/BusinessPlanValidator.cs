using FluentValidation;
using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Validation;

/// <summary>WO-42: FluentValidation for business plan API requests.</summary>
public sealed class CreateBusinessPlanRequestValidator : AbstractValidator<CreateBusinessPlanRequest>
{
    public CreateBusinessPlanRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
        RuleFor(x => x.Domain).MaximumLength(200).When(x => x.Domain is not null);
        RuleFor(x => x.DefaultSlaWindow!.WindowType).NotEmpty().MaximumLength(50)
            .When(x => x.DefaultSlaWindow is not null);
        RuleFor(x => x.DefaultSlaWindow!.WindowValue).GreaterThan(0)
            .When(x => x.DefaultSlaWindow is not null);
        RuleFor(x => x.DefaultSlaWindow!.AtRiskBufferMinutes).GreaterThanOrEqualTo(0)
            .When(x => x.DefaultSlaWindow is not null);
    }
}

public sealed class UpdateBusinessPlanRequestValidator : AbstractValidator<UpdateBusinessPlanRequest>
{
    public UpdateBusinessPlanRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200).When(x => x.Name is not null);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
        RuleFor(x => x.Domain).MaximumLength(200).When(x => x.Domain is not null);
        RuleFor(x => x.DefaultSlaWindow!.WindowType).NotEmpty().MaximumLength(50)
            .When(x => x.DefaultSlaWindow is not null);
        RuleFor(x => x.DefaultSlaWindow!.WindowValue).GreaterThan(0)
            .When(x => x.DefaultSlaWindow is not null);
        RuleFor(x => x.DefaultSlaWindow!.AtRiskBufferMinutes).GreaterThanOrEqualTo(0)
            .When(x => x.DefaultSlaWindow is not null);
    }
}

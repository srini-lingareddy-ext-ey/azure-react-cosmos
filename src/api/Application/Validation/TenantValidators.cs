using FluentValidation;
using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Validation;

/// <summary>WO-9: FluentValidation for tenant API requests.</summary>
public sealed class CreateTenantRequestValidator : AbstractValidator<CreateTenantRequest>
{
    public CreateTenantRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(500);
        RuleFor(x => x.LogoUrl).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.LogoUrl));
        RuleFor(x => x.BackgroundImageUrl).MaximumLength(2000).When(x => !string.IsNullOrEmpty(x.BackgroundImageUrl));
    }
}

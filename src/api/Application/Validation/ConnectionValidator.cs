using FluentValidation;
using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Validation;

/// <summary>WO-45: FluentValidation for connection API requests.</summary>
public sealed class CreateConnectionRequestValidator : AbstractValidator<CreateConnectionRequest>
{
    public CreateConnectionRequestValidator()
    {
        RuleFor(x => x.ConnectionName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ConnectorTypeId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Credentials).NotEmpty();
    }
}

public sealed class UpdateConnectionRequestValidator : AbstractValidator<UpdateConnectionRequest>
{
    public UpdateConnectionRequestValidator()
    {
        RuleFor(x => x.ConnectionName).MaximumLength(200).When(x => x.ConnectionName is not null);
        RuleFor(x => x.ConnectorTypeId).MaximumLength(100).When(x => x.ConnectorTypeId is not null);
    }
}

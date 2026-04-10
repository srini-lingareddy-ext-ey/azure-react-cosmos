using FluentValidation;
using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Validation;

/// <summary>WO-46: FluentValidation for query template API requests.</summary>
public sealed class CreateQueryTemplateRequestValidator : AbstractValidator<CreateQueryTemplateRequest>
{
    public CreateQueryTemplateRequestValidator()
    {
        RuleFor(x => x.TemplateName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ConnectorTypeId).NotEmpty().MaximumLength(100);
        RuleFor(x => x.TemplateBody).NotEmpty();
    }
}

public sealed class UpdateQueryTemplateRequestValidator : AbstractValidator<UpdateQueryTemplateRequest>
{
    public UpdateQueryTemplateRequestValidator()
    {
        RuleFor(x => x.TemplateName).MaximumLength(200).When(x => x.TemplateName is not null);
        RuleFor(x => x.PropagationMode)
            .Must(v => v == "allExisting" || v == "newOnly")
            .When(x => x.PropagationMode is not null)
            .WithMessage("PropagationMode must be 'allExisting' or 'newOnly'.");
    }
}

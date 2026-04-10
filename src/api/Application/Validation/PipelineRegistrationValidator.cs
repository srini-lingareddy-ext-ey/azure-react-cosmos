using FluentValidation;
using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Validation;

/// <summary>WO-43: FluentValidation for pipeline registration API requests.</summary>
public sealed class CreatePipelineRegistrationRequestValidator : AbstractValidator<CreatePipelineRegistrationRequest>
{
    public CreatePipelineRegistrationRequestValidator()
    {
        RuleFor(x => x.PipelineName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SourceSystem).MaximumLength(200).When(x => x.SourceSystem is not null);
        RuleFor(x => x.TargetSystem).MaximumLength(200).When(x => x.TargetSystem is not null);
        RuleFor(x => x.MedallionLayer).IsInEnum();
        RuleFor(x => x.Domain).MaximumLength(200).When(x => x.Domain is not null);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
    }
}

public sealed class UpdatePipelineRegistrationRequestValidator : AbstractValidator<UpdatePipelineRegistrationRequest>
{
    public UpdatePipelineRegistrationRequestValidator()
    {
        RuleFor(x => x.PipelineName).MaximumLength(200).When(x => x.PipelineName is not null);
        RuleFor(x => x.SourceSystem).MaximumLength(200).When(x => x.SourceSystem is not null);
        RuleFor(x => x.TargetSystem).MaximumLength(200).When(x => x.TargetSystem is not null);
        RuleFor(x => x.MedallionLayer!.Value).IsInEnum().When(x => x.MedallionLayer.HasValue);
        RuleFor(x => x.Domain).MaximumLength(200).When(x => x.Domain is not null);
        RuleFor(x => x.Description).MaximumLength(2000).When(x => x.Description is not null);
    }
}

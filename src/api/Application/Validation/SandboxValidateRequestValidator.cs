using FluentValidation;
using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Validation;

public sealed class SandboxValidateRequestValidator : TransportValidatorBase<SandboxValidateRequest>
{
    public SandboxValidateRequestValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required.")
            .MaximumLength(500)
            .WithMessage("Message must not exceed 500 characters.");
    }
}

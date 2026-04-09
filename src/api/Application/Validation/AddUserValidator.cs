using FluentValidation;
using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Validation;

/// <summary>WO-11: <see cref="AddUserRequest"/>.</summary>
public sealed class AddUserRequestValidator : AbstractValidator<AddUserRequest>
{
    public AddUserRequestValidator()
    {
        RuleFor(x => x.Role).IsInEnum();

        RuleFor(x => x)
            .Must(req =>
            {
                var hasUser = !string.IsNullOrWhiteSpace(req.UserId);
                var hasEmail = !string.IsNullOrWhiteSpace(req.Email);
                return hasUser ^ hasEmail;
            })
            .WithMessage("Provide either userId or email (exactly one, not both).");
    }
}

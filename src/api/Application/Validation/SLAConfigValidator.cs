using FluentValidation;
using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Validation;

public sealed class SLAConfigValidator : AbstractValidator<SLAConfigRequest>
{
    public SLAConfigValidator()
    {
        RuleFor(x => x.WindowType).Must(v => v == "absoluteTime" || v == "duration").WithMessage("windowType must be absoluteTime or duration");
        RuleFor(x => x.WindowValue).NotEmpty();
        RuleFor(x => x.AtRiskBufferMinutes).GreaterThan(0);
        RuleFor(x => x.WindowValue).Must((req, val) =>
        {
            if (req.WindowType == "absoluteTime") return TimeOnly.TryParse(val, out _);
            if (req.WindowType == "duration") return double.TryParse(val, out var d) && d > 0;
            return true;
        }).WithMessage("Invalid windowValue for the given windowType");
    }
}

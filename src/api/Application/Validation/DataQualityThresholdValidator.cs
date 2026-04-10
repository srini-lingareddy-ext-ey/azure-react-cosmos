using FluentValidation;
using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Validation;

public sealed class DataQualityThresholdValidator : AbstractValidator<DataQualityThresholdRequest>
{
    public DataQualityThresholdValidator()
    {
        RuleFor(x => x.CriticalThreshold).LessThan(x => x.WarningThreshold).WithMessage("criticalThreshold must be less than warningThreshold");
        RuleFor(x => x.WarningThreshold).InclusiveBetween(0, 100);
        RuleFor(x => x.CriticalThreshold).InclusiveBetween(0, 100);
    }
}

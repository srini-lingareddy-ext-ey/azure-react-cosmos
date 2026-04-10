using FluentValidation;
using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Validation;

/// <summary>WO-46: FluentValidation for monitor API requests.</summary>
public sealed class CreateMonitorRequestValidator : AbstractValidator<CreateMonitorRequest>
{
    public CreateMonitorRequestValidator()
    {
        RuleFor(x => x.MonitorName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityType).IsInEnum();
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.EntityName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ConnectionId).NotEmpty();
        RuleFor(x => x.PollingFrequencyMinutes).GreaterThanOrEqualTo(1);
    }
}

public sealed class UpdateMonitorRequestValidator : AbstractValidator<UpdateMonitorRequest>
{
    public UpdateMonitorRequestValidator()
    {
        RuleFor(x => x.MonitorName).MaximumLength(200).When(x => x.MonitorName is not null);
        RuleFor(x => x.PollingFrequencyMinutes!.Value).GreaterThanOrEqualTo(1).When(x => x.PollingFrequencyMinutes.HasValue);
    }
}

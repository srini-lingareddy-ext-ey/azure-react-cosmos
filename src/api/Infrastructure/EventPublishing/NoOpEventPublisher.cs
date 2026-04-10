using Todo.Api.Application.EventPublishing;

namespace Todo.Api.Infrastructure.EventPublishing;

/// <summary>WO-48: no-op publisher for local dev / when Event Hubs is not configured.</summary>
public sealed class NoOpEventPublisher : IEventPublisher
{
    private readonly ILogger<NoOpEventPublisher> _logger;
    public NoOpEventPublisher(ILogger<NoOpEventPublisher> logger) { _logger = logger; }

    public Task PublishAsync(string hubName, NormalizedEvent evt, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NoOpEventPublisher: would publish {EventType} to {Hub} for connector {ConnectorId}",
            evt.EventType, hubName, evt.ConnectorId);
        return Task.CompletedTask;
    }
}

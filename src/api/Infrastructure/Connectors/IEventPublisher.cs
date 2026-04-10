using Todo.Api.Application.Connectors;

namespace Todo.Api.Infrastructure.Connectors;

/// <summary>WO-48: abstraction over Event Hubs / Kafka publishing for testability.</summary>
public interface IEventPublisher
{
    Task PublishAsync(string hubName, NormalizedEvent evt, CancellationToken cancellationToken = default);
}

/// <summary>WO-48: no-op publisher for local dev / when Event Hubs is not configured.</summary>
public sealed class NoOpEventPublisher : IEventPublisher
{
    private readonly ILogger<NoOpEventPublisher> _logger;
    public NoOpEventPublisher(ILogger<NoOpEventPublisher> logger) { _logger = logger; }

    public Task PublishAsync(string hubName, NormalizedEvent evt, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("NoOpEventPublisher: would publish to {Hub} for connector {ConnectorId}", hubName, evt.ConnectorId);
        return Task.CompletedTask;
    }
}

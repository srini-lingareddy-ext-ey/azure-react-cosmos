namespace Todo.Api.Application.EventPublishing;

/// <summary>
/// WO-48: Application-layer abstraction over event publishing (Event Hubs / Kafka).
/// Connectors, evaluation jobs, and event processors call this interface —
/// they never reference Confluent.Kafka or Azure Event Hubs APIs directly.
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync(string hubName, NormalizedEvent evt, CancellationToken cancellationToken = default);
}

/// <summary>Canonical event envelope published through the streaming backbone.</summary>
public sealed class NormalizedEvent
{
    public string EventType { get; set; } = string.Empty;
    public string ConnectorId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

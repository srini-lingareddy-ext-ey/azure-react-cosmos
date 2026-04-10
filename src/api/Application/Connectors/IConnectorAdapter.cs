namespace Todo.Api.Application.Connectors;

/// <summary>WO-47: extensibility interface for connector adapters.</summary>
public interface IConnectorAdapter
{
    string ConnectorTypeId { get; }
    Domain.Entities.IntegrationMode SupportedMode { get; }
    Task<bool> TestConnectionAsync(string decryptedCredentials, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NormalizedEvent>> PollAsync(string decryptedCredentials, CancellationToken cancellationToken = default);
    NormalizedEvent NormalizeEvent(string rawPayload, string connectorId, string tenantId);
}

public sealed class NormalizedEvent
{
    public string EventType { get; set; } = string.Empty;
    public string ConnectorId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

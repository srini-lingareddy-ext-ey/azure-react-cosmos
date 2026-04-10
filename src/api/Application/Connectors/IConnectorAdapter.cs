using Todo.Api.Application.EventPublishing;

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

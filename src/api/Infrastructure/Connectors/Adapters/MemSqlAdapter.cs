using Todo.Api.Application.EventPublishing;
using Todo.Api.Application.Connectors;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Infrastructure.Connectors.Adapters;

/// <summary>WO-48: stub adapter for memsql (MVP — returns empty results).</summary>
public sealed class MemSqlAdapter : IConnectorAdapter
{
    public string ConnectorTypeId => "memsql";
    public IntegrationMode SupportedMode => IntegrationMode.Polling;

    public Task<bool> TestConnectionAsync(string decryptedCredentials, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public Task<IReadOnlyList<NormalizedEvent>> PollAsync(string decryptedCredentials, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<NormalizedEvent>>(Array.Empty<NormalizedEvent>());

    public NormalizedEvent NormalizeEvent(string rawPayload, string connectorId, string tenantId)
        => new() { EventType = "memsql", ConnectorId = connectorId, TenantId = tenantId, Payload = rawPayload };
}


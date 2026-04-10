using Todo.Api.Application.EventPublishing;
using Todo.Api.Application.Connectors;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Infrastructure.Connectors.Adapters;

/// <summary>WO-48: stub adapter for custom-webhook (MVP — returns empty results).</summary>
public sealed class CustomWebhookAdapter : IConnectorAdapter
{
    public string ConnectorTypeId => "custom-webhook";
    public IntegrationMode SupportedMode => IntegrationMode.Push;

    public Task<bool> TestConnectionAsync(string decryptedCredentials, CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public Task<IReadOnlyList<NormalizedEvent>> PollAsync(string decryptedCredentials, CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<NormalizedEvent>>(Array.Empty<NormalizedEvent>());

    public NormalizedEvent NormalizeEvent(string rawPayload, string connectorId, string tenantId)
        => new() { EventType = "custom-webhook", ConnectorId = connectorId, TenantId = tenantId, Payload = rawPayload };
}


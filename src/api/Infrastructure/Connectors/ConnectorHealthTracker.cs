using Todo.Api.Application.EventPublishing;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Connectors;

/// <summary>WO-48: updates ConnectorHealthStatus after each execution cycle.</summary>
public sealed class ConnectorHealthTracker
{
    private readonly IConnectorHealthStatusRepository _healthRepo;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ConnectorHealthTracker> _logger;

    public ConnectorHealthTracker(
        IConnectorHealthStatusRepository healthRepo,
        IEventPublisher eventPublisher,
        ILogger<ConnectorHealthTracker> logger)
    {
        _healthRepo = healthRepo;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task RecordSuccessAsync(string connectorId, string tenantId, int eventsProduced, CancellationToken cancellationToken = default)
    {
        var status = await _healthRepo.GetByConnectorIdAsync(connectorId, tenantId, cancellationToken).ConfigureAwait(false)
            ?? new ConnectorHealthStatus { Id = connectorId, TenantId = tenantId };

        status.ConnectorId = connectorId;
        status.Status = ConnectorHealthState.Active;
        status.ConsecutiveFailures = 0;
        status.LastSuccessfulExecutionAt = DateTimeOffset.UtcNow;
        status.EventsProducedLastCycle = eventsProduced;
        status.LastErrorMessage = null;
        status.UpdatedAt = DateTimeOffset.UtcNow;

        await _healthRepo.UpsertAsync(status, cancellationToken).ConfigureAwait(false);
    }

    public async Task RecordFailureAsync(string connectorId, string tenantId, string errorMessage, CancellationToken cancellationToken = default)
    {
        var status = await _healthRepo.GetByConnectorIdAsync(connectorId, tenantId, cancellationToken).ConfigureAwait(false)
            ?? new ConnectorHealthStatus { Id = connectorId, TenantId = tenantId };

        var previousState = status.Status;
        status.ConnectorId = connectorId;
        status.ConsecutiveFailures++;
        status.LastErrorMessage = errorMessage;
        status.EventsProducedLastCycle = 0;
        status.UpdatedAt = DateTimeOffset.UtcNow;

        status.Status = status.ConsecutiveFailures switch
        {
            >= 5 => ConnectorHealthState.Failed,
            >= 2 => ConnectorHealthState.Degraded,
            _ => ConnectorHealthState.Active,
        };

        await _healthRepo.UpsertAsync(status, cancellationToken).ConfigureAwait(false);

        // Publish platform alert events on threshold transitions
        if (status.Status == ConnectorHealthState.Degraded && previousState != ConnectorHealthState.Degraded)
        {
            await PublishAlertAsync("connector.health.degraded", connectorId, tenantId, status.ConsecutiveFailures, cancellationToken).ConfigureAwait(false);
            _logger.LogWarning("Connector {ConnectorId} degraded after {Failures} consecutive failures", connectorId, status.ConsecutiveFailures);
        }
        else if (status.Status == ConnectorHealthState.Failed && previousState != ConnectorHealthState.Failed)
        {
            await PublishAlertAsync("connector.health.failed", connectorId, tenantId, status.ConsecutiveFailures, cancellationToken).ConfigureAwait(false);
            _logger.LogError("Connector {ConnectorId} failed after {Failures} consecutive failures", connectorId, status.ConsecutiveFailures);
        }
    }

    private async Task PublishAlertAsync(string eventType, string connectorId, string tenantId, int consecutiveFailures, CancellationToken ct)
    {
        var evt = new NormalizedEvent
        {
            EventType = eventType,
            ConnectorId = connectorId,
            TenantId = tenantId,
            Payload = System.Text.Json.JsonSerializer.Serialize(new { connectorId, tenantId, consecutiveFailures, timestamp = DateTimeOffset.UtcNow }),
        };
        await _eventPublisher.PublishAsync("infrastructure-events", evt, ct).ConfigureAwait(false);
    }
}

using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Connectors;

/// <summary>WO-48: updates ConnectorHealthStatus after each execution cycle.</summary>
public sealed class ConnectorHealthTracker
{
    private readonly IConnectorHealthStatusRepository _healthRepo;

    public ConnectorHealthTracker(IConnectorHealthStatusRepository healthRepo)
    {
        _healthRepo = healthRepo;
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
    }
}

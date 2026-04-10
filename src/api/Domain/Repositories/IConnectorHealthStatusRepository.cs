using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

/// <summary>Persistence for <see cref="ConnectorHealthStatus"/> (WO-20).</summary>
public interface IConnectorHealthStatusRepository
{
    Task<ConnectorHealthStatus?> GetByConnectorIdAsync(string connectorId, string tenantId, CancellationToken cancellationToken = default);
    Task<ConnectorHealthStatus> UpsertAsync(ConnectorHealthStatus status, CancellationToken cancellationToken = default);
}

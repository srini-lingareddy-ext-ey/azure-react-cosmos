using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

/// <summary>Persistence for <see cref="ConnectorExecutionLog"/> (WO-20).</summary>
public interface IConnectorExecutionLogRepository
{
    Task<IReadOnlyList<ConnectorExecutionLog>> GetByConnectorIdAsync(string connectorId, string tenantId, int limit, CancellationToken cancellationToken = default);
    Task<ConnectorExecutionLog> CreateAsync(ConnectorExecutionLog log, CancellationToken cancellationToken = default);
}

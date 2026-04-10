using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed connector execution log repository (WO-20). Partition key /tenantId.</summary>
public sealed class ConnectorExecutionLogRepository : IConnectorExecutionLogRepository
{
    private readonly IRepository<ConnectorExecutionLog> _repository;
    public ConnectorExecutionLogRepository(IRepository<ConnectorExecutionLog> repository) { _repository = repository; }

    public async Task<IReadOnlyList<ConnectorExecutionLog>> GetByConnectorIdAsync(
        string connectorId, string tenantId, int limit, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec(
            $"SELECT * FROM c WHERE c.connectorId = @connectorId AND c.tenantId = @tenantId ORDER BY c.executedAt DESC OFFSET 0 LIMIT {limit}",
            new Dictionary<string, object> { ["@connectorId"] = connectorId, ["@tenantId"] = tenantId });
        var results = new List<ConnectorExecutionLog>();
        await foreach (var log in _repository.QueryAsync(spec, cancellationToken).ConfigureAwait(false))
            results.Add(log);
        return results;
    }

    public async Task<ConnectorExecutionLog> CreateAsync(ConnectorExecutionLog log, CancellationToken cancellationToken = default)
    {
        if (log.SchemaVersion == 0) log.SchemaVersion = 1;
        return await _repository.CreateAsync(log, cancellationToken).ConfigureAwait(false);
    }
}

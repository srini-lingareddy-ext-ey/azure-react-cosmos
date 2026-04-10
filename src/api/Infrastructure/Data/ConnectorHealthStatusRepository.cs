using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed connector health status repository (WO-20). Partition key /tenantId.</summary>
public sealed class ConnectorHealthStatusRepository : IConnectorHealthStatusRepository
{
    private readonly IRepository<ConnectorHealthStatus> _repository;
    public ConnectorHealthStatusRepository(IRepository<ConnectorHealthStatus> repository) { _repository = repository; }

    public Task<ConnectorHealthStatus?> GetByConnectorIdAsync(string connectorId, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(connectorId, tenantId, cancellationToken);

    public async Task<ConnectorHealthStatus> UpsertAsync(ConnectorHealthStatus status, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(status.Id, status.TenantId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            status.Etag = existing.Etag;
            return await _repository.UpdateAsync(status, cancellationToken).ConfigureAwait(false);
        }
        return await _repository.CreateAsync(status, cancellationToken).ConfigureAwait(false);
    }
}

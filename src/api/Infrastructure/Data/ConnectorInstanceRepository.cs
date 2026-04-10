using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed connector instance repository (WO-20). Partition key /tenantId.</summary>
public sealed class ConnectorInstanceRepository : IConnectorInstanceRepository
{
    private readonly IRepository<ConnectorInstance> _repository;
    public ConnectorInstanceRepository(IRepository<ConnectorInstance> repository) { _repository = repository; }

    public Task<ConnectorInstance?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, tenantId, cancellationToken);

    public IAsyncEnumerable<ConnectorInstance> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public IAsyncEnumerable<ConnectorInstance> GetAllEnabledAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId AND c.isEnabled = true",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public async Task<ConnectorInstance> CreateAsync(ConnectorInstance instance, CancellationToken cancellationToken = default)
    {
        if (instance.SchemaVersion == 0) instance.SchemaVersion = 1;
        return await _repository.CreateAsync(instance, cancellationToken).ConfigureAwait(false);
    }

    public Task<ConnectorInstance> UpdateAsync(ConnectorInstance instance, CancellationToken cancellationToken = default) =>
        _repository.UpdateAsync(instance, cancellationToken);
}

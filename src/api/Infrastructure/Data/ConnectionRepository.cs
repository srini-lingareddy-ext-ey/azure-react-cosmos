using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed connection repository (WO-18). Partition key /tenantId.</summary>
public sealed class ConnectionRepository : IConnectionRepository
{
    private readonly IRepository<Connection> _repository;
    public ConnectionRepository(IRepository<Connection> repository) { _repository = repository; }

    public Task<Connection?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, tenantId, cancellationToken);

    public async Task<Connection?> GetByNameAsync(string name, string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.connectionName = @name AND c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@name"] = name, ["@tenantId"] = tenantId });
        await foreach (var row in _repository.QueryAsync(spec, cancellationToken).ConfigureAwait(false))
            return row;
        return null;
    }

    public IAsyncEnumerable<Connection> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public async Task<Connection> CreateAsync(Connection connection, CancellationToken cancellationToken = default)
    {
        if (connection.SchemaVersion == 0) connection.SchemaVersion = 1;
        return await _repository.CreateAsync(connection, cancellationToken).ConfigureAwait(false);
    }

    public Task<Connection> UpdateAsync(Connection connection, CancellationToken cancellationToken = default) =>
        _repository.UpdateAsync(connection, cancellationToken);

    public Task DeleteAsync(string id, string tenantId, string? etag, CancellationToken cancellationToken = default) =>
        _repository.DeleteAsync(id, tenantId, etag, cancellationToken);
}

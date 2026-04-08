using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>
/// Cosmos-backed tenant repository (WO-4). Composes <see cref="IRepository{Tenant}"/> (partition key /id).
/// </summary>
public sealed class TenantRepository : ITenantRepository
{
    private readonly IRepository<Tenant> _repository;

    public TenantRepository(IRepository<Tenant> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public Task<Tenant?> GetByIdAsync(string id, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, id, cancellationToken);

    /// <inheritdoc />
    public async Task<Tenant?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec(
            "SELECT * FROM c WHERE c.name = @name",
            new Dictionary<string, object> { ["@name"] = name });
        await foreach (var tenant in _repository.QueryAsync(spec, cancellationToken).ConfigureAwait(false))
            return tenant;
        return null;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<Tenant> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c");
        return _repository.QueryAsync(spec, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        if (tenant.SchemaVersion == 0)
            tenant.SchemaVersion = 1;
        return await _repository.CreateAsync(tenant, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<Tenant> UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default) =>
        _repository.UpdateAsync(tenant, cancellationToken);
}

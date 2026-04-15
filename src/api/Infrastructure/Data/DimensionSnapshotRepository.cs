using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class DimensionSnapshotRepository : IDimensionSnapshotRepository
{
    private readonly IRepository<DimensionSnapshot> _repository;
    public DimensionSnapshotRepository(IRepository<DimensionSnapshot> repository) { _repository = repository; }

    public async Task<DimensionSnapshot?> GetLatestByDimensionAsync(string tenantId, string dimensionKey, CancellationToken ct = default)
    {
        var spec = new QuerySpec("SELECT TOP 1 * FROM c WHERE c.tenantId = @tenantId AND c.dimensionKey = @dimensionKey ORDER BY c.capturedAt DESC",
            new Dictionary<string, object> { ["@tenantId"] = tenantId, ["@dimensionKey"] = dimensionKey });
        await foreach (var row in _repository.QueryAsync(spec, ct).ConfigureAwait(false))
            return row;
        return null;
    }

    public IAsyncEnumerable<DimensionSnapshot> GetAllByTenantAsync(string tenantId, CancellationToken ct = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId ORDER BY c.capturedAt DESC",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, ct);
    }

    public Task<DimensionSnapshot> CreateAsync(DimensionSnapshot entity, CancellationToken ct = default) =>
        _repository.CreateAsync(entity, ct);
}

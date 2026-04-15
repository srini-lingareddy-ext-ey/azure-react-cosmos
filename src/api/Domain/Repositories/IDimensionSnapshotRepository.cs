using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IDimensionSnapshotRepository
{
    Task<DimensionSnapshot?> GetLatestByDimensionAsync(string tenantId, string dimensionKey, CancellationToken ct = default);
    IAsyncEnumerable<DimensionSnapshot> GetAllByTenantAsync(string tenantId, CancellationToken ct = default);
    Task<DimensionSnapshot> CreateAsync(DimensionSnapshot entity, CancellationToken ct = default);
}

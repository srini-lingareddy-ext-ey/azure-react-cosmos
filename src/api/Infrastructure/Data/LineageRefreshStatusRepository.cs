using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed lineage refresh status repository (WO-77). Partition key /tenantId. Id = tenantId.</summary>
public sealed class LineageRefreshStatusRepository : ILineageRefreshStatusRepository
{
    private readonly IRepository<LineageRefreshStatus> _repository;
    public LineageRefreshStatusRepository(IRepository<LineageRefreshStatus> repository) { _repository = repository; }

    public Task<LineageRefreshStatus?> GetByTenantIdAsync(string tenantId, CancellationToken ct = default) =>
        _repository.GetByIdAsync(tenantId, tenantId, ct);

    public Task<LineageRefreshStatus> UpsertAsync(LineageRefreshStatus entity, CancellationToken ct = default)
    {
        entity.Id = entity.TenantId;
        return _repository.UpsertAsync(entity, ct);
    }
}

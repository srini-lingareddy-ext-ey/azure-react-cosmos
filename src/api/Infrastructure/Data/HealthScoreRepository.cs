using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class HealthScoreRepository : IHealthScoreRepository
{
    private readonly IRepository<HealthScore> _repository;
    public HealthScoreRepository(IRepository<HealthScore> repository) { _repository = repository; }

    public Task<HealthScore?> GetByTenantIdAsync(string tenantId, CancellationToken ct = default) =>
        _repository.GetByIdAsync(tenantId, tenantId, ct);

    public Task<HealthScore> UpsertAsync(HealthScore entity, CancellationToken ct = default)
    {
        entity.Id = entity.TenantId;
        return _repository.UpsertAsync(entity, ct);
    }
}

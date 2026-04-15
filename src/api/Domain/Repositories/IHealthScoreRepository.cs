using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IHealthScoreRepository
{
    Task<HealthScore?> GetByTenantIdAsync(string tenantId, CancellationToken ct = default);
    Task<HealthScore> UpsertAsync(HealthScore entity, CancellationToken ct = default);
}

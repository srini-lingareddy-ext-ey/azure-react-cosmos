using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IDashboardSummaryRepository
{
    Task<DashboardSummary?> GetByTenantIdAsync(string tenantId, CancellationToken ct = default);
    Task<DashboardSummary> UpsertAsync(DashboardSummary entity, CancellationToken ct = default);
}

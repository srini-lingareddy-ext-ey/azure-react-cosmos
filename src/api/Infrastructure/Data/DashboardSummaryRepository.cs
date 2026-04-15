using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class DashboardSummaryRepository : IDashboardSummaryRepository
{
    private readonly IRepository<DashboardSummary> _repository;
    public DashboardSummaryRepository(IRepository<DashboardSummary> repository) { _repository = repository; }

    public Task<DashboardSummary?> GetByTenantIdAsync(string tenantId, CancellationToken ct = default) =>
        _repository.GetByIdAsync(tenantId, tenantId, ct);

    public Task<DashboardSummary> UpsertAsync(DashboardSummary entity, CancellationToken ct = default)
    {
        entity.Id = entity.TenantId;
        return _repository.UpsertAsync(entity, ct);
    }
}

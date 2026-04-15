using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface ILineageRefreshStatusRepository
{
    Task<LineageRefreshStatus?> GetByTenantIdAsync(string tenantId, CancellationToken ct = default);
    Task<LineageRefreshStatus> UpsertAsync(LineageRefreshStatus entity, CancellationToken ct = default);
}

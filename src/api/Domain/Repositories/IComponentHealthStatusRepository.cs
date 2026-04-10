using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IComponentHealthStatusRepository
{
    Task<ComponentHealthStatus?> GetByIdAsync(string componentId, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ComponentHealthStatus> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<ComponentHealthStatus> UpsertAsync(ComponentHealthStatus entity, CancellationToken cancellationToken = default);
}

using Todo.Api.Domain.Entities;
using Monitor = Todo.Api.Domain.Entities.Monitor;

namespace Todo.Api.Domain.Repositories;

/// <summary>Persistence for <see cref="Monitor"/> (WO-19).</summary>
public interface IMonitorRepository
{
    Task<Monitor?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Monitor> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Monitor> GetByConnectionAsync(string connectionId, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Monitor> GetByEntityAsync(string entityId, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Monitor> GetByBusinessPlanAsync(string businessPlanId, string tenantId, CancellationToken cancellationToken = default);
    Task<Monitor> CreateAsync(Monitor monitor, CancellationToken cancellationToken = default);
    Task<Monitor> UpdateAsync(Monitor monitor, CancellationToken cancellationToken = default);
    Task PauseByPipelineAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default);
}

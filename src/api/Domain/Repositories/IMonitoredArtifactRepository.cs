using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IMonitoredArtifactRepository
{
    Task<MonitoredArtifact?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<MonitoredArtifact> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<MonitoredArtifact> GetAllActiveByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<MonitoredArtifact> CreateAsync(MonitoredArtifact entity, CancellationToken cancellationToken = default);
    Task<MonitoredArtifact> UpdateAsync(MonitoredArtifact entity, CancellationToken cancellationToken = default);
}

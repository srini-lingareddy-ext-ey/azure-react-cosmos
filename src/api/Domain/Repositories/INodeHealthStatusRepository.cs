using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface INodeHealthStatusRepository
{
    Task<NodeHealthStatus?> GetByIdAsync(string nodeId, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<NodeHealthStatus> GetByComponentIdAsync(string componentId, string tenantId, CancellationToken cancellationToken = default);
    Task<NodeHealthStatus> UpsertAsync(NodeHealthStatus entity, CancellationToken cancellationToken = default);
}

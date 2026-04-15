using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface ILineageNodeRepository
{
    Task<LineageNode?> GetByNodeIdAsync(string nodeId, string tenantId, CancellationToken ct = default);
    IAsyncEnumerable<LineageNode> GetAllByTenantAsync(string tenantId, CancellationToken ct = default);
    Task BulkUpsertAsync(IEnumerable<LineageNode> nodes, string tenantId, CancellationToken ct = default);
    Task DeleteAllByTenantAsync(string tenantId, CancellationToken ct = default);
}

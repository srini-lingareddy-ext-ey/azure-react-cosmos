using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed lineage node repository (WO-77). Partition key /tenantId.</summary>
public sealed class LineageNodeRepository : ILineageNodeRepository
{
    private readonly IRepository<LineageNode> _repository;
    public LineageNodeRepository(IRepository<LineageNode> repository) { _repository = repository; }

    public Task<LineageNode?> GetByNodeIdAsync(string nodeId, string tenantId, CancellationToken ct = default) =>
        _repository.GetByIdAsync(nodeId, tenantId, ct);

    public IAsyncEnumerable<LineageNode> GetAllByTenantAsync(string tenantId, CancellationToken ct = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, ct);
    }

    public async Task BulkUpsertAsync(IEnumerable<LineageNode> nodes, string tenantId, CancellationToken ct = default)
    {
        foreach (var node in nodes)
        {
            node.Id = node.NodeId;
            node.TenantId = tenantId;
            await _repository.UpsertAsync(node, ct).ConfigureAwait(false);
        }
    }

    public async Task DeleteAllByTenantAsync(string tenantId, CancellationToken ct = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        await foreach (var node in _repository.QueryAsync(spec, ct).ConfigureAwait(false))
            await _repository.DeleteAsync(node.Id, tenantId, cancellationToken: ct).ConfigureAwait(false);
    }
}

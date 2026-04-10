using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class NodeHealthStatusRepository : INodeHealthStatusRepository
{
    private readonly IRepository<NodeHealthStatus> _repository;
    public NodeHealthStatusRepository(IRepository<NodeHealthStatus> repository) { _repository = repository; }

    public Task<NodeHealthStatus?> GetByIdAsync(string nodeId, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(nodeId, tenantId, cancellationToken);

    public IAsyncEnumerable<NodeHealthStatus> GetByComponentIdAsync(string componentId, string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.componentId = @componentId AND c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@componentId"] = componentId, ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public Task<NodeHealthStatus> UpsertAsync(NodeHealthStatus entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id)) entity.Id = entity.NodeId;
        return _repository.UpsertAsync(entity, cancellationToken);
    }
}

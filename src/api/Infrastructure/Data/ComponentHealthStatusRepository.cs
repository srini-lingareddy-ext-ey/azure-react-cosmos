using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class ComponentHealthStatusRepository : IComponentHealthStatusRepository
{
    private readonly IRepository<ComponentHealthStatus> _repository;
    public ComponentHealthStatusRepository(IRepository<ComponentHealthStatus> repository) { _repository = repository; }

    public Task<ComponentHealthStatus?> GetByIdAsync(string componentId, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(componentId, tenantId, cancellationToken);

    public IAsyncEnumerable<ComponentHealthStatus> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public Task<ComponentHealthStatus> UpsertAsync(ComponentHealthStatus entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id)) entity.Id = entity.ComponentId;
        return _repository.UpsertAsync(entity, cancellationToken);
    }
}

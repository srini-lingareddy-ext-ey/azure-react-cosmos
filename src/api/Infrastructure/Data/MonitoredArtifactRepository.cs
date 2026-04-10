using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class MonitoredArtifactRepository : IMonitoredArtifactRepository
{
    private readonly IRepository<MonitoredArtifact> _repository;
    public MonitoredArtifactRepository(IRepository<MonitoredArtifact> repository) { _repository = repository; }

    public Task<MonitoredArtifact?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, tenantId, cancellationToken);

    public IAsyncEnumerable<MonitoredArtifact> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId", new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public IAsyncEnumerable<MonitoredArtifact> GetAllActiveByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId AND c.isActive = true", new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public Task<MonitoredArtifact> CreateAsync(MonitoredArtifact entity, CancellationToken cancellationToken = default) =>
        _repository.CreateAsync(entity, cancellationToken);

    public Task<MonitoredArtifact> UpdateAsync(MonitoredArtifact entity, CancellationToken cancellationToken = default) =>
        _repository.UpsertAsync(entity, cancellationToken);
}

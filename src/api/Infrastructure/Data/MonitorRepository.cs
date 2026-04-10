using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Monitor = Todo.Api.Domain.Entities.Monitor;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed monitor repository (WO-19). Partition key /tenantId.</summary>
public sealed class MonitorRepository : IMonitorRepository
{
    private readonly IRepository<Monitor> _repository;
    public MonitorRepository(IRepository<Monitor> repository) { _repository = repository; }

    public Task<Monitor?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, tenantId, cancellationToken);

    public IAsyncEnumerable<Monitor> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public IAsyncEnumerable<Monitor> GetByConnectionAsync(string connectionId, string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.connectionId = @connectionId AND c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@connectionId"] = connectionId, ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public IAsyncEnumerable<Monitor> GetByEntityAsync(string entityId, string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.entityId = @entityId AND c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@entityId"] = entityId, ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public IAsyncEnumerable<Monitor> GetByBusinessPlanAsync(string businessPlanId, string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.businessPlanId = @businessPlanId AND c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@businessPlanId"] = businessPlanId, ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public async Task<Monitor> CreateAsync(Monitor monitor, CancellationToken cancellationToken = default)
    {
        if (monitor.SchemaVersion == 0) monitor.SchemaVersion = 1;
        return await _repository.CreateAsync(monitor, cancellationToken).ConfigureAwait(false);
    }

    public Task<Monitor> UpdateAsync(Monitor monitor, CancellationToken cancellationToken = default) =>
        _repository.UpdateAsync(monitor, cancellationToken);

    public async Task PauseByPipelineAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.entityId = @pipelineId AND c.tenantId = @tenantId AND c.status = @activeStatus",
            new Dictionary<string, object>
            {
                ["@pipelineId"] = pipelineId,
                ["@tenantId"] = tenantId,
                ["@activeStatus"] = (int)MonitorState.Active,
            });
        await foreach (var monitor in _repository.QueryAsync(spec, cancellationToken).ConfigureAwait(false))
        {
            monitor.Status = MonitorState.Paused;
            await _repository.UpdateAsync(monitor, cancellationToken).ConfigureAwait(false);
        }
    }
}

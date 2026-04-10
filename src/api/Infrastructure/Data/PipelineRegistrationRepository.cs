using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed pipeline registration repository (WO-15). Partition key /tenantId.</summary>
public sealed class PipelineRegistrationRepository : IPipelineRegistrationRepository
{
    private readonly IRepository<PipelineRegistration> _repository;
    public PipelineRegistrationRepository(IRepository<PipelineRegistration> repository) { _repository = repository; }

    public Task<PipelineRegistration?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, tenantId, cancellationToken);

    public async Task<PipelineRegistration?> GetByNameAsync(string name, string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.pipelineName = @pipelineName AND c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@pipelineName"] = name, ["@tenantId"] = tenantId });
        await foreach (var row in _repository.QueryAsync(spec, cancellationToken).ConfigureAwait(false))
            return row;
        return null;
    }

    public IAsyncEnumerable<PipelineRegistration> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public IAsyncEnumerable<PipelineRegistration> GetByBusinessPlanAsync(string businessPlanId, string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.businessPlanId = @businessPlanId AND c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@businessPlanId"] = businessPlanId, ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public async Task<PipelineRegistration> CreateAsync(PipelineRegistration registration, CancellationToken cancellationToken = default)
    {
        if (registration.SchemaVersion == 0) registration.SchemaVersion = 1;
        return await _repository.CreateAsync(registration, cancellationToken).ConfigureAwait(false);
    }

    public Task<PipelineRegistration> UpdateAsync(PipelineRegistration registration, CancellationToken cancellationToken = default) =>
        _repository.UpdateAsync(registration, cancellationToken);
}

using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed pipeline lineage repository (WO-16). Partition key /tenantId.</summary>
public sealed class PipelineLineageRepository : IPipelineLineageRepository
{
    private readonly IRepository<PipelineLineageRelationship> _repository;
    public PipelineLineageRepository(IRepository<PipelineLineageRelationship> repository) { _repository = repository; }

    public Task<PipelineLineageRelationship?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, tenantId, cancellationToken);

    public IAsyncEnumerable<PipelineLineageRelationship> GetByUpstreamPipelineAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.upstreamPipelineId = @pipelineId AND c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@pipelineId"] = pipelineId, ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public IAsyncEnumerable<PipelineLineageRelationship> GetByDownstreamPipelineAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.downstreamPipelineId = @pipelineId AND c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@pipelineId"] = pipelineId, ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public IAsyncEnumerable<PipelineLineageRelationship> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public async Task<PipelineLineageRelationship> CreateAsync(PipelineLineageRelationship relationship, CancellationToken cancellationToken = default)
    {
        if (relationship.SchemaVersion == 0) relationship.SchemaVersion = 1;
        return await _repository.CreateAsync(relationship, cancellationToken).ConfigureAwait(false);
    }

    public Task DeleteAsync(string id, string tenantId, string? etag = null, CancellationToken cancellationToken = default) =>
        _repository.DeleteAsync(id, tenantId, etag, cancellationToken);
}

using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class PipelineSLAConfigRepository : IPipelineSLAConfigRepository
{
    private readonly IRepository<PipelineSLAConfig> _repository;
    public PipelineSLAConfigRepository(IRepository<PipelineSLAConfig> repository) { _repository = repository; }

    public Task<PipelineSLAConfig?> GetByPipelineIdAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(pipelineId, tenantId, cancellationToken);

    public IAsyncEnumerable<PipelineSLAConfig> GetAllWithConfigByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public Task<PipelineSLAConfig> UpsertAsync(PipelineSLAConfig entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id)) entity.Id = entity.PipelineId;
        return _repository.UpsertAsync(entity, cancellationToken);
    }
}

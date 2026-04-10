using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class PipelineSLAStatusRepository : IPipelineSLAStatusRepository
{
    private readonly IRepository<PipelineSLAStatus> _repository;
    public PipelineSLAStatusRepository(IRepository<PipelineSLAStatus> repository) { _repository = repository; }

    public Task<PipelineSLAStatus?> GetByPipelineIdAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(pipelineId, tenantId, cancellationToken);

    public IAsyncEnumerable<PipelineSLAStatus> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public Task<PipelineSLAStatus> UpsertAsync(PipelineSLAStatus entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id)) entity.Id = entity.PipelineId;
        return _repository.UpsertAsync(entity, cancellationToken);
    }
}

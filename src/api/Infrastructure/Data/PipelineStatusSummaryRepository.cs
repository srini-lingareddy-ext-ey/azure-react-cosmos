using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class PipelineStatusSummaryRepository : IPipelineStatusSummaryRepository
{
    private readonly IRepository<PipelineStatusSummary> _repository;
    public PipelineStatusSummaryRepository(IRepository<PipelineStatusSummary> repository) { _repository = repository; }

    public Task<PipelineStatusSummary?> GetByIdAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(pipelineId, tenantId, cancellationToken);

    public IAsyncEnumerable<PipelineStatusSummary> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public Task<PipelineStatusSummary> UpsertAsync(PipelineStatusSummary entity, CancellationToken cancellationToken = default) =>
        _repository.UpsertAsync(entity, cancellationToken);
}

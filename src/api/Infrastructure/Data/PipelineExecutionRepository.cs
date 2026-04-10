using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class PipelineExecutionRepository : IPipelineExecutionRepository
{
    private readonly IRepository<PipelineExecution> _repository;
    public PipelineExecutionRepository(IRepository<PipelineExecution> repository) { _repository = repository; }

    public Task<PipelineExecution?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, tenantId, cancellationToken);

    public async Task<PipelineExecution?> GetLatestByPipelineAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT TOP 1 * FROM c WHERE c.pipelineId = @pipelineId AND c.tenantId = @tenantId ORDER BY c.startedAt DESC",
            new Dictionary<string, object> { ["@pipelineId"] = pipelineId, ["@tenantId"] = tenantId });
        await foreach (var row in _repository.QueryAsync(spec, cancellationToken).ConfigureAwait(false))
            return row;
        return null;
    }

    public Task<PipelineExecution> CreateAsync(PipelineExecution entity, CancellationToken cancellationToken = default) =>
        _repository.CreateAsync(entity, cancellationToken);

    public IAsyncEnumerable<PipelineExecution> GetByPipelineAsync(string pipelineId, string tenantId, int limit = 20, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec($"SELECT TOP {limit} * FROM c WHERE c.pipelineId = @pipelineId AND c.tenantId = @tenantId ORDER BY c.startedAt DESC",
            new Dictionary<string, object> { ["@pipelineId"] = pipelineId, ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }
}

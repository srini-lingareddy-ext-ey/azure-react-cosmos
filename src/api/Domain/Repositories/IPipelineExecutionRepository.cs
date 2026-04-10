using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IPipelineExecutionRepository
{
    Task<PipelineExecution?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    Task<PipelineExecution?> GetLatestByPipelineAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default);
    Task<PipelineExecution> CreateAsync(PipelineExecution entity, CancellationToken cancellationToken = default);
    IAsyncEnumerable<PipelineExecution> GetByPipelineAsync(string pipelineId, string tenantId, int limit = 20, CancellationToken cancellationToken = default);
}

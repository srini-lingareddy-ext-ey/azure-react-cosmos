using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IPipelineSLAStatusRepository
{
    Task<PipelineSLAStatus?> GetByPipelineIdAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<PipelineSLAStatus> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<PipelineSLAStatus> UpsertAsync(PipelineSLAStatus entity, CancellationToken cancellationToken = default);
}

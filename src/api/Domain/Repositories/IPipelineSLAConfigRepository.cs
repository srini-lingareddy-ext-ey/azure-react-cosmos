using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IPipelineSLAConfigRepository
{
    Task<PipelineSLAConfig?> GetByPipelineIdAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<PipelineSLAConfig> GetAllWithConfigByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<PipelineSLAConfig> UpsertAsync(PipelineSLAConfig entity, CancellationToken cancellationToken = default);
}

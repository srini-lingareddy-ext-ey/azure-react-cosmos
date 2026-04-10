using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IPipelineStatusSummaryRepository
{
    Task<PipelineStatusSummary?> GetByIdAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<PipelineStatusSummary> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<PipelineStatusSummary> UpsertAsync(PipelineStatusSummary entity, CancellationToken cancellationToken = default);
}

using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IPipelineSLABreachRecordRepository
{
    Task<PipelineSLABreachRecord> CreateAsync(PipelineSLABreachRecord entity, CancellationToken cancellationToken = default);
    IAsyncEnumerable<PipelineSLABreachRecord> GetByPipelineIdAsync(string pipelineId, string tenantId, int limit = 30, CancellationToken cancellationToken = default);
    Task UpdateCompletedAtAsync(string id, string tenantId, DateTimeOffset completedAt, int minutesOverdue, CancellationToken cancellationToken = default);
}

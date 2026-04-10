using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IJobLongRunThresholdRepository
{
    Task<JobLongRunThreshold?> GetByJobAsync(string pipelineId, string jobName, string tenantId, CancellationToken cancellationToken = default);
    Task<JobLongRunThreshold> UpsertAsync(JobLongRunThreshold entity, CancellationToken cancellationToken = default);
    IAsyncEnumerable<JobLongRunThreshold> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
}

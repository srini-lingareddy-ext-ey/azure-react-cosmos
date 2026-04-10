using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IJobRunRepository
{
    Task<JobRun?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<JobRun> GetByExecutionIdAsync(string executionId, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<JobRun> GetByJobNameAndPipelineAsync(string pipelineId, string jobName, string tenantId, int days = 30, CancellationToken cancellationToken = default);
    Task<JobRun> CreateAsync(JobRun entity, CancellationToken cancellationToken = default);
}

using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class JobRunRepository : IJobRunRepository
{
    private readonly IRepository<JobRun> _repository;
    public JobRunRepository(IRepository<JobRun> repository) { _repository = repository; }

    public Task<JobRun?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, tenantId, cancellationToken);

    public IAsyncEnumerable<JobRun> GetByExecutionIdAsync(string executionId, string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.executionId = @executionId AND c.tenantId = @tenantId ORDER BY c.startTime DESC",
            new Dictionary<string, object> { ["@executionId"] = executionId, ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public IAsyncEnumerable<JobRun> GetByJobNameAndPipelineAsync(string pipelineId, string jobName, string tenantId, int days = 30, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTimeOffset.UtcNow.AddDays(-days).ToString("o");
        var spec = new QuerySpec("SELECT * FROM c WHERE c.pipelineId = @pipelineId AND c.jobName = @jobName AND c.tenantId = @tenantId AND c.startTime >= @cutoff ORDER BY c.startTime DESC",
            new Dictionary<string, object> { ["@pipelineId"] = pipelineId, ["@jobName"] = jobName, ["@tenantId"] = tenantId, ["@cutoff"] = cutoff });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public Task<JobRun> CreateAsync(JobRun entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
            entity.Id = $"{entity.ExecutionId}_{entity.JobName}";
        entity.CreatedAt ??= DateTimeOffset.UtcNow;
        return _repository.CreateAsync(entity, cancellationToken);
    }
}

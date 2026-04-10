using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class JobLongRunThresholdRepository : IJobLongRunThresholdRepository
{
    private readonly IRepository<JobLongRunThreshold> _repository;
    public JobLongRunThresholdRepository(IRepository<JobLongRunThreshold> repository) { _repository = repository; }

    public async Task<JobLongRunThreshold?> GetByJobAsync(string pipelineId, string jobName, string tenantId, CancellationToken cancellationToken = default)
    {
        var id = $"{tenantId}_{pipelineId}_{jobName}";
        return await _repository.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
    }

    public Task<JobLongRunThreshold> UpsertAsync(JobLongRunThreshold entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id))
            entity.Id = $"{entity.TenantId}_{entity.PipelineId}_{entity.JobName}";
        return _repository.UpsertAsync(entity, cancellationToken);
    }

    public IAsyncEnumerable<JobLongRunThreshold> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }
}

using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class PipelineSLABreachRecordRepository : IPipelineSLABreachRecordRepository
{
    private readonly IRepository<PipelineSLABreachRecord> _repository;
    public PipelineSLABreachRecordRepository(IRepository<PipelineSLABreachRecord> repository) { _repository = repository; }

    public Task<PipelineSLABreachRecord> CreateAsync(PipelineSLABreachRecord entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id)) entity.Id = Guid.NewGuid().ToString();
        return _repository.CreateAsync(entity, cancellationToken);
    }

    public IAsyncEnumerable<PipelineSLABreachRecord> GetByPipelineIdAsync(string pipelineId, string tenantId, int limit = 30, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec($"SELECT TOP {limit} * FROM c WHERE c.pipelineId = @pipelineId AND c.tenantId = @tenantId ORDER BY c.breachDetectedAt DESC",
            new Dictionary<string, object> { ["@pipelineId"] = pipelineId, ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public async Task UpdateCompletedAtAsync(string id, string tenantId, DateTimeOffset completedAt, int minutesOverdue, CancellationToken cancellationToken = default)
    {
        var record = await _repository.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (record is null) return;
        record.CompletedAt = completedAt;
        record.MinutesOverdue = minutesOverdue;
        await _repository.UpdateAsync(record, cancellationToken).ConfigureAwait(false);
    }
}

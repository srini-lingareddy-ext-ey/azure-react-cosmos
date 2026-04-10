using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class DataQualityThresholdConfigRepository : IDataQualityThresholdConfigRepository
{
    private readonly IRepository<DataQualityThresholdConfig> _repository;
    public DataQualityThresholdConfigRepository(IRepository<DataQualityThresholdConfig> repository) { _repository = repository; }

    public Task<DataQualityThresholdConfig?> GetByPipelineIdAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(pipelineId, tenantId, cancellationToken);

    public Task<DataQualityThresholdConfig> UpsertAsync(DataQualityThresholdConfig entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id)) entity.Id = entity.PipelineId;
        return _repository.UpsertAsync(entity, cancellationToken);
    }

    public IAsyncEnumerable<DataQualityThresholdConfig> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }
}

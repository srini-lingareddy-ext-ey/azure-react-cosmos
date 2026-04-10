using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IDataQualityThresholdConfigRepository
{
    Task<DataQualityThresholdConfig?> GetByPipelineIdAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default);
    Task<DataQualityThresholdConfig> UpsertAsync(DataQualityThresholdConfig entity, CancellationToken cancellationToken = default);
    IAsyncEnumerable<DataQualityThresholdConfig> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
}

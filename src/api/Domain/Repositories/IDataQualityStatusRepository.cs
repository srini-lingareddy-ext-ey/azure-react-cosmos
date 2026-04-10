using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IDataQualityStatusRepository
{
    Task<DataQualityStatus?> GetByPipelineIdAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<DataQualityStatus> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<DataQualityStatus> UpsertAsync(DataQualityStatus entity, CancellationToken cancellationToken = default);
}

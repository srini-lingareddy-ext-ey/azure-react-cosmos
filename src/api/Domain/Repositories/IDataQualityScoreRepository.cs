using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IDataQualityScoreRepository
{
    Task<DataQualityScore?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<DataQualityScore> GetByPipelineAndDateRangeAsync(string pipelineId, string tenantId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default);
    Task<DataQualityScore> CreateAsync(DataQualityScore entity, CancellationToken cancellationToken = default);
}

using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class DataQualityScoreRepository : IDataQualityScoreRepository
{
    private readonly IRepository<DataQualityScore> _repository;
    public DataQualityScoreRepository(IRepository<DataQualityScore> repository) { _repository = repository; }

    public Task<DataQualityScore?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, tenantId, cancellationToken);

    public IAsyncEnumerable<DataQualityScore> GetByPipelineAndDateRangeAsync(string pipelineId, string tenantId, DateTimeOffset startDate, DateTimeOffset endDate, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.pipelineId = @pipelineId AND c.tenantId = @tenantId AND c.runAt >= @startDate AND c.runAt <= @endDate ORDER BY c.runAt DESC",
            new Dictionary<string, object> { ["@pipelineId"] = pipelineId, ["@tenantId"] = tenantId, ["@startDate"] = startDate.ToString("o"), ["@endDate"] = endDate.ToString("o") });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public Task<DataQualityScore> CreateAsync(DataQualityScore entity, CancellationToken cancellationToken = default) =>
        _repository.CreateAsync(entity, cancellationToken);
}

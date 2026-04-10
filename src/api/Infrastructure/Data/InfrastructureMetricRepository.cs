using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class InfrastructureMetricRepository : IInfrastructureMetricRepository
{
    private readonly IRepository<InfrastructureMetric> _repository;
    public InfrastructureMetricRepository(IRepository<InfrastructureMetric> repository) { _repository = repository; }

    public IAsyncEnumerable<InfrastructureMetric> GetRecentByNodeAndMetricAsync(string nodeId, string metricName, string tenantId, int limit = 60, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec($"SELECT TOP {limit} * FROM c WHERE c.nodeId = @nodeId AND c.metricName = @metricName AND c.tenantId = @tenantId ORDER BY c.recordedAt DESC",
            new Dictionary<string, object> { ["@nodeId"] = nodeId, ["@metricName"] = metricName, ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public Task<InfrastructureMetric> CreateAsync(InfrastructureMetric entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id)) entity.Id = Guid.NewGuid().ToString();
        return _repository.CreateAsync(entity, cancellationToken);
    }
}

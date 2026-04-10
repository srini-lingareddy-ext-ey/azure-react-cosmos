using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IInfrastructureMetricRepository
{
    IAsyncEnumerable<InfrastructureMetric> GetRecentByNodeAndMetricAsync(string nodeId, string metricName, string tenantId, int limit = 60, CancellationToken cancellationToken = default);
    Task<InfrastructureMetric> CreateAsync(InfrastructureMetric entity, CancellationToken cancellationToken = default);
}

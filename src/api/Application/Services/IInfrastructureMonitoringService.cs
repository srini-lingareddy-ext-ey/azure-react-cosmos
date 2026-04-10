using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

public interface IInfrastructureMonitoringService
{
    Task<InfrastructureStatusResponse> GetStatusAsync(string tenantId, string? status, CancellationToken ct);
    Task<List<NodeStatusDto>?> GetComponentNodesAsync(string tenantId, string componentId, CancellationToken ct);
    Task<List<NodeMetricDto>> GetNodeMetricsAsync(string tenantId, string nodeId, CancellationToken ct);
    Task<ProductAvailabilityResponse?> GetProductAvailabilityAsync(string tenantId, string productId, int trendDays, CancellationToken ct);
}

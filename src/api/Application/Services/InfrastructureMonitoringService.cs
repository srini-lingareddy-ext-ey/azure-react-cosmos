using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

public sealed class InfrastructureMonitoringService : IInfrastructureMonitoringService
{
    private readonly IComponentHealthStatusRepository _componentRepo;
    private readonly INodeHealthStatusRepository _nodeRepo;
    private readonly IInfrastructureMetricRepository _metricRepo;
    private readonly IInfraThresholdConfigRepository _configRepo;
    private readonly IProductAvailabilityRepository _productRepo;

    public InfrastructureMonitoringService(
        IComponentHealthStatusRepository componentRepo, INodeHealthStatusRepository nodeRepo,
        IInfrastructureMetricRepository metricRepo, IInfraThresholdConfigRepository configRepo,
        IProductAvailabilityRepository productRepo)
    {
        _componentRepo = componentRepo;
        _nodeRepo = nodeRepo;
        _metricRepo = metricRepo;
        _configRepo = configRepo;
        _productRepo = productRepo;
    }

    public async Task<InfrastructureStatusResponse> GetStatusAsync(string tenantId, string? status, CancellationToken ct)
    {
        var components = new List<ComponentHealthDto>();
        await foreach (var c in _componentRepo.GetAllByTenantAsync(tenantId, ct))
        {
            if (status is not null && !string.Equals(c.Status.ToString(), status, StringComparison.OrdinalIgnoreCase)) continue;
            components.Add(new ComponentHealthDto(c.ComponentId, c.ComponentName, c.ComponentType.ToString(), c.Status.ToString(), c.LastMetricReceivedAt, c.IsStale, c.NodeCount, c.UnhealthyNodeCount, c.EvaluatedAt));
        }
        var products = new List<ProductHealthDto>();
        await foreach (var p in _productRepo.GetAllByTenantAsync(tenantId, ct))
        {
            var isStale = p.LastHeartbeatAt.HasValue && (DateTimeOffset.UtcNow - p.LastHeartbeatAt.Value).TotalMinutes > 15;
            if (status is not null && !string.Equals(p.Status.ToString(), status, StringComparison.OrdinalIgnoreCase)) continue;
            products.Add(new ProductHealthDto(p.ProductId, p.ProductName, p.Availability24h, isStale ? "Unknown" : p.Status.ToString(), p.LastHeartbeatAt, isStale));
        }
        return new InfrastructureStatusResponse(components, products);
    }

    public async Task<List<NodeStatusDto>?> GetComponentNodesAsync(string tenantId, string componentId, CancellationToken ct)
    {
        var component = await _componentRepo.GetByIdAsync(componentId, tenantId, ct);
        if (component is null) return null;
        var nodes = new List<NodeStatusDto>();
        await foreach (var n in _nodeRepo.GetByComponentIdAsync(componentId, tenantId, ct))
            nodes.Add(new NodeStatusDto(n.NodeId, n.NodeName, n.Status.ToString(), n.LastMetricReceivedAt, n.IsStale));
        return nodes;
    }

    public async Task<List<NodeMetricDto>> GetNodeMetricsAsync(string tenantId, string nodeId, CancellationToken ct)
    {
        var node = await _nodeRepo.GetByIdAsync(nodeId, tenantId, ct);
        if (node is null) return new List<NodeMetricDto>();

        var config = await _configRepo.GetByComponentIdAsync(node.ComponentId, tenantId, ct);
        var metricNames = config?.Thresholds.Keys.ToList() ?? new List<string> { "cpu_utilization", "memory_usage", "disk_io" };

        var result = new List<NodeMetricDto>();
        foreach (var metricName in metricNames)
        {
            var sparkline = new List<MetricSparklinePointDto>();
            double? currentValue = null;
            await foreach (var m in _metricRepo.GetRecentByNodeAndMetricAsync(nodeId, metricName, tenantId, 60, ct))
            {
                currentValue ??= m.Value;
                sparkline.Add(new MetricSparklinePointDto(m.RecordedAt, m.Value));
            }
            sparkline.Reverse();

            var threshold = config?.Thresholds.GetValueOrDefault(metricName);
            var metricStatus = currentValue.HasValue && threshold is not null
                ? currentValue.Value >= threshold.CriticalThreshold ? "Critical"
                : currentValue.Value >= threshold.WarningThreshold ? "Warning" : "Healthy"
                : "Healthy";

            result.Add(new NodeMetricDto(metricName, threshold?.DisplayName, threshold?.Unit, currentValue, threshold?.WarningThreshold, threshold?.CriticalThreshold, metricStatus, sparkline));
        }
        return result;
    }

    public async Task<ProductAvailabilityResponse?> GetProductAvailabilityAsync(string tenantId, string productId, int trendDays, CancellationToken ct)
    {
        var product = await _productRepo.GetByProductIdAsync(productId, tenantId, ct);
        if (product is null) return null;
        var trend = product.DailyAvailability.TakeLast(trendDays).Select(d => new ProductAvailabilityTrendDto(d.Date, d.AvailabilityPercent)).ToList();
        return new ProductAvailabilityResponse(product.Availability24h, product.Status.ToString(), trend);
    }
}

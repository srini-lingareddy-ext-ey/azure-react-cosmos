using System.Text.Json;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.EventProcessing;

public sealed class InfrastructureMetricEventHandler : IEventHandler
{
    public string EventType => "infrastructure.metric";

    private readonly IInfrastructureMetricRepository _metricRepo;
    private readonly INodeHealthStatusRepository _nodeRepo;
    private readonly IInfraThresholdConfigRepository _configRepo;
    private readonly ILogger<InfrastructureMetricEventHandler> _logger;

    public InfrastructureMetricEventHandler(
        IInfrastructureMetricRepository metricRepo,
        INodeHealthStatusRepository nodeRepo,
        IInfraThresholdConfigRepository configRepo,
        ILogger<InfrastructureMetricEventHandler> logger)
    {
        _metricRepo = metricRepo;
        _nodeRepo = nodeRepo;
        _configRepo = configRepo;
        _logger = logger;
    }

    public async Task HandleAsync(string payload, CancellationToken ct)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(payload);
        var tenantId = data.GetProperty("tenantId").GetString() ?? string.Empty;
        var nodeId = data.GetProperty("nodeId").GetString() ?? string.Empty;
        var componentId = data.GetProperty("componentId").GetString() ?? string.Empty;
        var metricName = data.GetProperty("metricName").GetString() ?? string.Empty;
        var value = data.TryGetProperty("value", out var v) ? v.GetDouble() : 0;

        var metric = new InfrastructureMetric
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            NodeId = nodeId,
            ComponentId = componentId,
            MetricName = metricName,
            Value = value,
            Unit = data.TryGetProperty("unit", out var u) ? u.GetString() : null,
            RecordedAt = data.TryGetProperty("recordedAt", out var ra) ? ra.GetDateTimeOffset() : DateTimeOffset.UtcNow,
        };

        await _metricRepo.CreateAsync(metric, ct).ConfigureAwait(false);

        var config = await _configRepo.GetByComponentIdAsync(componentId, tenantId, ct).ConfigureAwait(false);
        if (config is null || !config.Thresholds.TryGetValue(metricName, out var threshold))
        {
            _logger.LogDebug("No threshold config for component {ComponentId} metric {MetricName}, skipping node status update", componentId, metricName);
            return;
        }

        var nodeStatus = value >= threshold.CriticalThreshold ? InfraHealthState.Critical
            : value >= threshold.WarningThreshold ? InfraHealthState.Warning
            : InfraHealthState.Healthy;

        var node = await _nodeRepo.GetByIdAsync(nodeId, tenantId, ct).ConfigureAwait(false)
            ?? new NodeHealthStatus { Id = nodeId, TenantId = tenantId, ComponentId = componentId, NodeId = nodeId };

        node.NodeName = data.TryGetProperty("nodeName", out var nn) ? nn.GetString() ?? nodeId : nodeId;
        node.Status = nodeStatus;
        node.LastMetricReceivedAt = metric.RecordedAt;
        node.IsStale = false;
        node.UpdatedAt = DateTimeOffset.UtcNow;

        await _nodeRepo.UpsertAsync(node, ct).ConfigureAwait(false);
        _logger.LogDebug("Processed infra metric {MetricName}={Value} for node {NodeId}, status={Status}", metricName, value, nodeId, nodeStatus);
    }
}

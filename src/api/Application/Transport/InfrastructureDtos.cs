namespace Todo.Api.Application.Transport;

public sealed record ComponentHealthDto(string ComponentId, string ComponentName, string ComponentType, string Status, DateTimeOffset? LastMetricReceivedAt, bool IsStale, int NodeCount, int UnhealthyNodeCount, DateTimeOffset? EvaluatedAt);
public sealed record ProductHealthDto(string ProductId, string ProductName, double Availability24h, string Status, DateTimeOffset? LastHeartbeatAt, bool IsStale);
public sealed record InfrastructureStatusResponse(List<ComponentHealthDto> Components, List<ProductHealthDto> Products);
public sealed record NodeStatusDto(string NodeId, string NodeName, string Status, DateTimeOffset? LastMetricReceivedAt, bool IsStale);
public sealed record MetricSparklinePointDto(DateTimeOffset Timestamp, double Value);
public sealed record NodeMetricDto(string MetricName, string? DisplayName, string? Unit, double? CurrentValue, double? WarningThreshold, double? CriticalThreshold, string Status, List<MetricSparklinePointDto> Sparkline);
public sealed record ProductAvailabilityTrendDto(string Date, double AvailabilityPercent);
public sealed record ProductAvailabilityResponse(double Availability24h, string Status, List<ProductAvailabilityTrendDto> Trend);

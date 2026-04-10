namespace Todo.Api.Domain.Entities;

public enum QualityStatus { Passing = 0, Warning = 1, Failing = 2, NoData = 3 }
public enum LatencyStatus { Fresh = 0, Approaching = 1, Stale = 2, NoData = 3 }

public sealed class DataQualityStatus : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PipelineId { get; set; } = string.Empty;
    public string PipelineName { get; set; } = string.Empty;
    public string? BusinessPlan { get; set; }
    public string? Domain { get; set; }
    public string? LatestScoreId { get; set; }
    public double? QualityScore { get; set; }
    public DateTimeOffset? ScoreTimestamp { get; set; }
    public double? WarningThreshold { get; set; }
    public double? CriticalThreshold { get; set; }
    public QualityStatus QualityStatusValue { get; set; } = QualityStatus.NoData;
    public DateTimeOffset? LastSuccessfulRunAt { get; set; }
    public long? FreshnessThresholdSeconds { get; set; }
    public double? FreshnessBufferPercent { get; set; }
    public LatencyStatus LatencyStatusValue { get; set; } = LatencyStatus.NoData;
    public DateTimeOffset? EvaluatedAt { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

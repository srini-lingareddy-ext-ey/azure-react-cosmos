namespace Todo.Api.Domain.Entities;

public sealed class DataQualityThresholdConfig : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PipelineId { get; set; } = string.Empty;
    public double? WarningThreshold { get; set; }
    public double? CriticalThreshold { get; set; }
    public long? FreshnessThresholdSeconds { get; set; }
    public double? FreshnessBufferPercent { get; set; }
    public DateTimeOffset? ConfiguredAt { get; set; }
    public string? ConfiguredBy { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

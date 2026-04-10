namespace Todo.Api.Domain.Entities;

public sealed class ThresholdEntry
{
    public double WarningThreshold { get; set; }
    public double CriticalThreshold { get; set; }
    public string? Unit { get; set; }
    public string? DisplayName { get; set; }
}

public sealed class InfraThresholdConfig : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ComponentId { get; set; } = string.Empty;
    public Dictionary<string, ThresholdEntry> Thresholds { get; set; } = new();
    public double? AvailabilityThreshold { get; set; }
    public long? StalenessThresholdSeconds { get; set; }
    public DateTimeOffset? ConfiguredAt { get; set; }
    public string? ConfiguredBy { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

namespace Todo.Api.Domain.Entities;

public enum SLAStatus { OnTrack = 0, AtRisk = 1, Breached = 2, Met = 3 }

public sealed class PipelineSLAStatus : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PipelineId { get; set; } = string.Empty;
    public string PipelineName { get; set; } = string.Empty;
    public string? BusinessPlan { get; set; }
    public SLAStatus Status { get; set; }
    public double? TimeRemainingSeconds { get; set; }
    public DateTimeOffset? LastRunAt { get; set; }
    public DateTimeOffset? EvaluatedAt { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

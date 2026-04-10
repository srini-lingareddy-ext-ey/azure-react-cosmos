namespace Todo.Api.Domain.Entities;

public enum SLAWindowType { AbsoluteTime = 0, Duration = 1 }

public sealed class PipelineSLAConfig : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PipelineId { get; set; } = string.Empty;
    public SLAWindowType WindowType { get; set; }
    public string WindowValue { get; set; } = string.Empty;
    public int AtRiskBufferMinutes { get; set; }
    public DateTimeOffset? ConfiguredAt { get; set; }
    public string? ConfiguredBy { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

namespace Todo.Api.Domain.Entities;

public sealed class PipelineSLABreachRecord : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PipelineId { get; set; } = string.Empty;
    public string PipelineName { get; set; } = string.Empty;
    public string? BusinessPlan { get; set; }
    public DateTimeOffset SlaWindowClosedAt { get; set; }
    public DateTimeOffset BreachDetectedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public int? MinutesOverdue { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

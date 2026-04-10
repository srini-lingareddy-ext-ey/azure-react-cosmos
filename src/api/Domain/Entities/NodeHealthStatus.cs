namespace Todo.Api.Domain.Entities;

public sealed class NodeHealthStatus : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ComponentId { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public InfraHealthState Status { get; set; } = InfraHealthState.Unknown;
    public DateTimeOffset? LastMetricReceivedAt { get; set; }
    public bool IsStale { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

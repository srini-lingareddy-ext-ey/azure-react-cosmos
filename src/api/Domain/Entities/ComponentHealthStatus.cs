namespace Todo.Api.Domain.Entities;

public enum ComponentType { Server = 0, Database = 1, Broker = 2, Storage = 3, Network = 4, Custom = 5 }
public enum InfraHealthState { Healthy = 0, Warning = 1, Critical = 2, Unknown = 3 }

public sealed class ComponentHealthStatus : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ComponentId { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public ComponentType ComponentType { get; set; }
    public InfraHealthState Status { get; set; } = InfraHealthState.Unknown;
    public DateTimeOffset? LastMetricReceivedAt { get; set; }
    public bool IsStale { get; set; }
    public int NodeCount { get; set; }
    public int UnhealthyNodeCount { get; set; }
    public DateTimeOffset? EvaluatedAt { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

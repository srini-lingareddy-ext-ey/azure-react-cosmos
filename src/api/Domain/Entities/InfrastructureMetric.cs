namespace Todo.Api.Domain.Entities;

public sealed class InfrastructureMetric : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string ComponentId { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public string? Unit { get; set; }
    public DateTimeOffset RecordedAt { get; set; }
    public int Ttl { get; set; } = 7200;
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

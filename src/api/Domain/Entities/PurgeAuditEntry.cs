namespace Todo.Api.Domain.Entities;

public sealed class PurgeAuditEntry : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTimeOffset? PurgedAt { get; set; }
    public int DeletedCount { get; set; }
    public int RetentionDaysApplied { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

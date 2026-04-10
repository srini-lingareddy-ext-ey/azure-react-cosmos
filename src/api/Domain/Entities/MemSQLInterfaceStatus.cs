namespace Todo.Api.Domain.Entities;

public sealed class MemSQLInterfaceStatus : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string InterfaceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long PendingRecordCount { get; set; }
    public DateTimeOffset? LastCompletedAt { get; set; }
    public string? LastErrorMessage { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

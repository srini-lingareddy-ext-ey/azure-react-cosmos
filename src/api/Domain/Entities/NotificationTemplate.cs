namespace Todo.Api.Domain.Entities;

public sealed class NotificationTemplate : IDomainEntity, IConcurrencyEntity, IAuditableEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = "platform";
    public string Classification { get; set; } = string.Empty;
    public string SubjectTemplate { get; set; } = string.Empty;
    public string BodyTemplate { get; set; } = string.Empty;
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public object PartitionKeyValue => TenantId;
}
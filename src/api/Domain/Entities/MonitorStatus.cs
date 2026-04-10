namespace Todo.Api.Domain.Entities;

/// <summary>
/// Runtime status document for a monitor (WO-19). Container <c>monitor-status</c>, partition <c>/tenantId</c>.
/// Id equals MonitorId for efficient point-reads. Does not implement IAuditableEntity.
/// </summary>
public sealed class MonitorStatus : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string MonitorId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int ConsecutiveErrors { get; set; }

    public DateTimeOffset? LastSuccessfulExecutionAt { get; set; }

    public DateTimeOffset? NextScheduledExecutionAt { get; set; }

    public string? LastErrorMessage { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public int SchemaVersion { get; set; } = 1;

    public string? Etag { get; set; }

    /// <inheritdoc />
    public object PartitionKeyValue => TenantId;
}

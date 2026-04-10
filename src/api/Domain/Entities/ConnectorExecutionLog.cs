namespace Todo.Api.Domain.Entities;

/// <summary>Execution outcome for a connector run (WO-20).</summary>
public enum ExecutionStatus
{
    Success = 0,
    Failed = 1,
    Partial = 2,
}

/// <summary>
/// Connector execution log entry (WO-20). Does not implement IAuditableEntity.
/// Container <c>connector-execution-log</c>, partition <c>/tenantId</c>.
/// </summary>
public sealed class ConnectorExecutionLog : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string ConnectorId { get; set; } = string.Empty;

    public DateTimeOffset ExecutedAt { get; set; }

    public ExecutionStatus Status { get; set; }

    public int EventsProduced { get; set; }

    public long DurationMs { get; set; }

    public string? ErrorMessage { get; set; }

    public int Ttl { get; set; } = 2592000;

    public int SchemaVersion { get; set; } = 1;

    public string? Etag { get; set; }

    public object PartitionKeyValue => TenantId;
}

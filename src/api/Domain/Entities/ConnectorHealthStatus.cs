namespace Todo.Api.Domain.Entities;

/// <summary>Health state for a connector (WO-20).</summary>
public enum ConnectorHealthState
{
    Active = 0,
    Degraded = 1,
    Failed = 2,
    Disabled = 3,
}

/// <summary>
/// Connector health status document (WO-20). Upserted status doc; does not implement IAuditableEntity.
/// Container <c>connector-health-status</c>, partition <c>/tenantId</c>.
/// </summary>
public sealed class ConnectorHealthStatus : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string ConnectorId { get; set; } = string.Empty;

    public ConnectorHealthState Status { get; set; }

    public int ConsecutiveFailures { get; set; }

    public DateTimeOffset? LastSuccessfulExecutionAt { get; set; }

    public int EventsProducedLastCycle { get; set; }

    public string? LastErrorMessage { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }

    public int SchemaVersion { get; set; } = 1;

    public string? Etag { get; set; }

    public object PartitionKeyValue => TenantId;
}

namespace Todo.Api.Domain.Entities;

/// <summary>Type of entity being monitored (WO-19).</summary>
public enum MonitorEntityType
{
    Pipeline = 0,
    InfrastructureComponent = 1,
}

/// <summary>Operational state of a monitor (WO-19).</summary>
public enum MonitorState
{
    Active = 0,
    Paused = 1,
    Error = 2,
}

/// <summary>
/// Monitor aggregate root (WO-19). Container <c>monitor</c>, partition <c>/tenantId</c>.
/// </summary>
public sealed class Monitor : IDomainEntity, IAuditableEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string MonitorName { get; set; } = string.Empty;

    public MonitorEntityType EntityType { get; set; }

    public string EntityId { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string? BusinessPlanId { get; set; }

    public string? BusinessPlanName { get; set; }

    public string ConnectionId { get; set; } = string.Empty;

    public string ConnectionName { get; set; } = string.Empty;

    public string? QueryTemplateId { get; set; }

    public string? QueryTemplateSnapshot { get; set; }

    public int PollingFrequencyMinutes { get; set; }

    public List<AlertThreshold> AlertThresholds { get; set; } = new();

    public MonitorState Status { get; set; } = MonitorState.Active;

    public int SchemaVersion { get; set; } = 1;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Etag { get; set; }

    /// <inheritdoc />
    public object PartitionKeyValue => TenantId;

    /// <summary>Threshold configuration for a single metric.</summary>
    public sealed class AlertThreshold
    {
        public string MetricName { get; set; } = string.Empty;

        public double WarningValue { get; set; }

        public double CriticalValue { get; set; }

        public string Operator { get; set; } = string.Empty;

        public string Unit { get; set; } = string.Empty;
    }
}

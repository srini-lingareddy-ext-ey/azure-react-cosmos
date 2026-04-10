namespace Todo.Api.Domain.Entities;

/// <summary>
/// Business plan entity for tenant-scoped data governance (WO-15).
/// Container <c>business-plan</c>, partition <c>/tenantId</c>.
/// </summary>
public sealed class BusinessPlan : IDomainEntity, IAuditableEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Domain { get; set; }

    public bool IsActive { get; set; }

    public SLAWindowConfig? DefaultSlaWindow { get; set; }

    public int SchemaVersion { get; set; } = 1;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Etag { get; set; }

    /// <inheritdoc />
    public object PartitionKeyValue => TenantId;

    /// <summary>SLA window configuration for a business plan.</summary>
    public sealed class SLAWindowConfig
    {
        public string WindowType { get; set; } = string.Empty;

        public int WindowValue { get; set; }

        public int AtRiskBufferMinutes { get; set; }
    }
}

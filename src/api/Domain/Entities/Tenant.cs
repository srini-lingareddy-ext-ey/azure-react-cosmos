namespace Todo.Api.Domain.Entities;

/// <summary>
/// Tenant aggregate root for configuration (WO-4). Stored in Cosmos container <c>tenant</c>, partition key <c>/id</c>.
/// </summary>
public sealed class Tenant : IDomainEntity, IAuditableEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;

    public object PartitionKeyValue => Id;

    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public TenantStatus Status { get; set; } = TenantStatus.Active;

    public TenantBranding? Branding { get; set; }

    public TenantConfig? Config { get; set; }

    /// <summary>Schema version for forward-compatible migrations; default 1 on create.</summary>
    public int SchemaVersion { get; set; } = 1;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Etag { get; set; }
}

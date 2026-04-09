namespace Todo.Api.Domain.Entities;

/// <summary>
/// WO-11: email invitation to join a tenant. Container <c>user-invitation</c>, partition <c>/tenantId</c>.
/// <see cref="Ttl"/> is Cosmos DB time-to-live (seconds) for automatic cleanup after retention.
/// </summary>
public sealed class UserInvitation : IDomainEntity, IAuditableEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public string InvitedBy { get; set; } = string.Empty;

    public DateTimeOffset InvitedAt { get; set; }

    public DateTimeOffset ExpiresAt { get; set; }

    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;

    public DateTimeOffset? AcceptedAt { get; set; }

    public int SchemaVersion { get; set; } = 1;

    /// <summary>Cosmos DB TTL in seconds (30 days). Document removed by the service after expiry.</summary>
    public int? Ttl { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Etag { get; set; }

    /// <inheritdoc />
    public object PartitionKeyValue => TenantId;
}

namespace Todo.Api.Domain.Entities;

/// <summary>
/// Links a user to a tenant with a role (WO-5). Container <c>user-role-assignment</c>, partition <c>/tenantId</c>.
/// <see cref="PartitionKeyValue"/> is <see cref="TenantId"/> (not document id) so point operations partition correctly.
/// </summary>
public sealed class UserRoleAssignment : IDomainEntity, IAuditableEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;

    /// <summary>Partition key for Cosmos; must match container partition path <c>/tenantId</c>.</summary>
    public string TenantId { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? DisplayName { get; set; }

    public UserRole Role { get; set; }

    public UserStatus Status { get; set; } = UserStatus.Active;

    public int SchemaVersion { get; set; } = 1;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Etag { get; set; }

    /// <inheritdoc />
    public object PartitionKeyValue => TenantId;
}

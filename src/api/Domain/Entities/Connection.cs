namespace Todo.Api.Domain.Entities;

/// <summary>
/// External data-source connection (WO-18). Container <c>connection</c>, partition <c>/tenantId</c>.
/// </summary>
public sealed class Connection : IDomainEntity, IAuditableEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string ConnectionName { get; set; } = string.Empty;

    public string ConnectorTypeId { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    /// <summary>Encrypted credentials blob; never returned in API responses.</summary>
    public string CredentialsEncrypted { get; set; } = string.Empty;

    public DateTimeOffset? LastTestedAt { get; set; }

    public string? LastTestResult { get; set; }

    public int SchemaVersion { get; set; } = 1;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Etag { get; set; }

    /// <inheritdoc />
    public object PartitionKeyValue => TenantId;
}

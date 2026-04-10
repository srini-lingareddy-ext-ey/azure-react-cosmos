namespace Todo.Api.Domain.Entities;

/// <summary>
/// Reusable query template for a connector type (WO-18). Container <c>query-template</c>, partition <c>/tenantId</c>.
/// </summary>
public sealed class QueryTemplate : IDomainEntity, IAuditableEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string TemplateName { get; set; } = string.Empty;

    public string ConnectorTypeId { get; set; } = string.Empty;

    public string TemplateBody { get; set; } = string.Empty;

    /// <summary>Parameter placeholders stored as a JSON array in Cosmos.</summary>
    public string[] Parameters { get; set; } = [];

    public bool IsActive { get; set; }

    public int SchemaVersion { get; set; } = 1;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Etag { get; set; }

    /// <inheritdoc />
    public object PartitionKeyValue => TenantId;
}

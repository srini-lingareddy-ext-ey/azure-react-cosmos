namespace Todo.Api.Domain.Entities;

/// <summary>Integration mode for a connector (WO-20).</summary>
public enum IntegrationMode
{
    Polling = 0,
    Push = 1,
}

/// <summary>Transform type applied during field mapping (WO-20).</summary>
public enum TransformType
{
    Direct = 0,
    ValueMap = 1,
}

/// <summary>
/// Connector instance aggregate root (WO-20). Container <c>connector-instance</c>, partition <c>/tenantId</c>.
/// </summary>
public sealed class ConnectorInstance : IDomainEntity, IAuditableEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string ConnectorName { get; set; } = string.Empty;

    public string ConnectorTypeId { get; set; } = string.Empty;

    public bool IsEnabled { get; set; }

    public IntegrationMode IntegrationMode { get; set; }

    public string? PollingScheduleCron { get; set; }

    public string CredentialsEncrypted { get; set; } = string.Empty;

    public List<FieldMapping> FieldMappings { get; set; } = new();

    public int SchemaVersion { get; set; } = 1;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Etag { get; set; }

    public object PartitionKeyValue => TenantId;

    /// <summary>Maps a source field to a target field with an optional value transformation.</summary>
    public sealed class FieldMapping
    {
        public string SourceField { get; set; } = string.Empty;

        public string TargetField { get; set; } = string.Empty;

        public TransformType TransformType { get; set; }

        public Dictionary<string, string>? ValueMap { get; set; }
    }
}

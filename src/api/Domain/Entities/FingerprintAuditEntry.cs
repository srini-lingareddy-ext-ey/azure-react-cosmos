namespace Todo.Api.Domain.Entities;

public enum ChangeClassification { Authorized = 0, Unauthorized = 1 }

public sealed class FingerprintAuditEntry : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ArtifactId { get; set; } = string.Empty;
    public string ArtifactName { get; set; } = string.Empty;
    public ArtifactType ArtifactType { get; set; }
    public DateTimeOffset? DetectedAt { get; set; }
    public string ChangedBy { get; set; } = "unknown";
    public string BeforeHash { get; set; } = string.Empty;
    public string AfterHash { get; set; } = string.Empty;
    public ChangeClassification ChangeClassification { get; set; }
    public string? ApprovedWindowId { get; set; }
    public string? ApprovedWindowName { get; set; }
    public string? BaselineResetJustification { get; set; }
    public bool SyncedToImmutableStorage { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

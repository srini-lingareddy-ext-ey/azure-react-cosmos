namespace Todo.Api.Domain.Entities;

public sealed class ArtifactFingerprint : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ArtifactId { get; set; } = string.Empty;
    public string ApprovedHash { get; set; } = string.Empty;
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public bool IsInitialBaseline { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

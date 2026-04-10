namespace Todo.Api.Domain.Entities;

public enum WindowScopeType { All = 0, ArtifactType = 1, Artifact = 2 }

public sealed class FingerprintApprovedWindow : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public WindowScopeType ScopeType { get; set; }
    public string? ScopeValue { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

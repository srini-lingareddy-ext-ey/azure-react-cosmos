namespace Todo.Api.Domain.Entities;

public enum ArtifactType { PipelineDefinition = 0, DatabaseSchema = 1, TransformationScript = 2, ConfigFile = 3 }
public enum ArtifactStatus { Baseline = 0, Deviated = 1, Unknown = 2 }

public sealed class MonitoredArtifact : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ArtifactName { get; set; } = string.Empty;
    public ArtifactType ArtifactType { get; set; }
    public string ConnectorId { get; set; } = string.Empty;
    public Dictionary<string, string> RetrievalConfig { get; set; } = new();
    public string ScanScheduleCron { get; set; } = "0 2 * * *";
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastScannedAt { get; set; }
    public DateTimeOffset? LastDeviationDetectedAt { get; set; }
    public ArtifactStatus CurrentStatus { get; set; } = ArtifactStatus.Unknown;
    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

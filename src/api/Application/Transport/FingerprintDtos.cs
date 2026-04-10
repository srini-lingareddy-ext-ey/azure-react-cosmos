namespace Todo.Api.Application.Transport;

public sealed class MonitoredArtifactView
{
    public string ArtifactId { get; set; } = string.Empty;
    public string ArtifactName { get; set; } = string.Empty;
    public string ArtifactType { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public DateTimeOffset? LastScannedAt { get; set; }
    public DateTimeOffset? LastDeviationDetectedAt { get; set; }
}

public sealed class RegisterArtifactRequest
{
    public string ArtifactName { get; set; } = string.Empty;
    public string ArtifactType { get; set; } = string.Empty;
    public string ConnectorId { get; set; } = string.Empty;
    public Dictionary<string, string> RetrievalConfig { get; set; } = new();
    public string? ScanScheduleCron { get; set; }
}

public sealed class ResetBaselineRequest
{
    public string Justification { get; set; } = string.Empty;
}

public sealed class CreateApprovedWindowRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public string ScopeType { get; set; } = "all";
    public string? ScopeValue { get; set; }
}

public sealed class FingerprintAuditEntryView
{
    public string Id { get; set; } = string.Empty;
    public string ArtifactId { get; set; } = string.Empty;
    public string ArtifactName { get; set; } = string.Empty;
    public string ArtifactType { get; set; } = string.Empty;
    public DateTimeOffset? DetectedAt { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public string BeforeHash { get; set; } = string.Empty;
    public string AfterHash { get; set; } = string.Empty;
    public string ChangeClassification { get; set; } = string.Empty;
    public string? ApprovedWindowName { get; set; }
    public bool SyncedToImmutableStorage { get; set; }
}

public sealed class ApprovedWindowView
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public string ScopeType { get; set; } = string.Empty;
    public string? ScopeValue { get; set; }
}

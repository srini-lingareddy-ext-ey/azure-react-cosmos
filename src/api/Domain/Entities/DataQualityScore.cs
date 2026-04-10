namespace Todo.Api.Domain.Entities;

public sealed class QualityCheckResult
{
    public string CheckName { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public long RecordsEvaluated { get; set; }
    public long RecordsFailed { get; set; }
    public double FailureRate { get; set; }
    public string? Message { get; set; }
}

public sealed class DataQualityScore : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PipelineId { get; set; } = string.Empty;
    public string PipelineName { get; set; } = string.Empty;
    public string? BusinessPlan { get; set; }
    public double OverallScore { get; set; }
    public DateTimeOffset? RunAt { get; set; }
    public DateTimeOffset? IngestedAt { get; set; }
    public List<QualityCheckResult> Checks { get; set; } = new();
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

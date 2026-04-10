namespace Todo.Api.Domain.Entities;

public sealed class JobRun : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PipelineId { get; set; } = string.Empty;
    public string PipelineName { get; set; } = string.Empty;
    public string ExecutionId { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
    public double? DurationSeconds { get; set; }
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }
    public int RetryCount { get; set; }
    public bool IsLongRunning { get; set; }
    public string? SourceSystemUrl { get; set; }
    public bool HasGranularData { get; set; }
    public bool IsSkipped { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public int Ttl { get; set; } = 7776000;
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

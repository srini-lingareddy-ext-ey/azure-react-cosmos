namespace Todo.Api.Domain.Entities;

public sealed class JobLongRunThreshold : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PipelineId { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public double? ThresholdSeconds { get; set; }
    public double? AverageDurationSeconds { get; set; }
    public int CalculatedFromRuns { get; set; }
    public bool IsApplicable { get; set; }
    public DateTimeOffset? CalculatedAt { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

namespace Todo.Api.Domain.Entities;

public sealed class HopSummary
{
    public string Layer { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool HasDetail { get; set; }
}

public sealed class PipelineStatusSummary : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PipelineId { get; set; } = string.Empty;
    public string PipelineName { get; set; } = string.Empty;
    public string? BusinessPlan { get; set; }
    public string? Domain { get; set; }
    public string? Layer { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? LastRunAt { get; set; }
    public string? LatestExecutionId { get; set; }
    public List<HopSummary> Hops { get; set; } = new();
    public DateTimeOffset? UpdatedAt { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

namespace Todo.Api.Domain.Entities;

/// <summary>
/// Directed pipeline-to-pipeline edge (WO-16). Container <c>pipeline-lineage-relationship</c>, partition <c>/tenantId</c>.
/// </summary>
public sealed class PipelineLineageRelationship : IDomainEntity, IAuditableEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string UpstreamPipelineId { get; set; } = string.Empty;

    public string UpstreamPipelineName { get; set; } = string.Empty;

    public string DownstreamPipelineId { get; set; } = string.Empty;

    public string DownstreamPipelineName { get; set; } = string.Empty;

    public int SchemaVersion { get; set; } = 1;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Etag { get; set; }

    /// <inheritdoc />
    public object PartitionKeyValue => TenantId;
}

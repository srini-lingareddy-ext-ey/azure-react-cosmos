namespace Todo.Api.Application.Transport;

/// <summary>WO-44: POST /api/v1/admin/lineage</summary>
public sealed class CreateLineageRequest
{
    public string UpstreamPipelineId { get; set; } = string.Empty;
    public string DownstreamPipelineId { get; set; } = string.Empty;
}

/// <summary>WO-44: single lineage relationship response.</summary>
public sealed class LineageRelationshipResponse
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string UpstreamPipelineId { get; set; } = string.Empty;
    public string UpstreamPipelineName { get; set; } = string.Empty;
    public string DownstreamPipelineId { get; set; } = string.Empty;
    public string DownstreamPipelineName { get; set; } = string.Empty;
    public DateTimeOffset? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>WO-44: lineage view for a single pipeline.</summary>
public sealed class PipelineLineageResponse
{
    public string PipelineId { get; set; } = string.Empty;
    public IReadOnlyList<LineageEdgeDto> Upstream { get; set; } = Array.Empty<LineageEdgeDto>();
    public IReadOnlyList<LineageEdgeDto> Downstream { get; set; } = Array.Empty<LineageEdgeDto>();
}

public sealed class LineageEdgeDto
{
    public string RelationshipId { get; set; } = string.Empty;
    public string RelatedPipelineId { get; set; } = string.Empty;
    public string RelatedPipelineName { get; set; } = string.Empty;
}

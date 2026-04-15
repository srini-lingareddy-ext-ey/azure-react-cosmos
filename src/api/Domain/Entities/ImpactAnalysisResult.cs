namespace Todo.Api.Domain.Entities;

public enum ImpactAnalysisStatus { Pending = 0, Complete = 1, Unavailable = 2 }
public enum ImpactNodeStatus { Healthy = 0, AtRisk = 1, Failed = 2, Unknown = 3 }

public sealed class ImpactNode
{
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public LineageNodeType NodeType { get; set; }
    public ImpactNodeStatus CurrentStatus { get; set; } = ImpactNodeStatus.Unknown;
    public int Depth { get; set; }
}

public sealed class ImpactAnalysisResult : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string? IncidentId { get; set; }
    public string FailedNodeId { get; set; } = string.Empty;
    public LineageNodeType FailedNodeType { get; set; }
    public ImpactAnalysisStatus Status { get; set; } = ImpactAnalysisStatus.Pending;
    public DateTimeOffset? TraversedAt { get; set; }
    public List<ImpactNode> Upstream { get; set; } = new();
    public List<ImpactNode> Downstream { get; set; } = new();
    public bool AdditionalUpstreamExist { get; set; }
    public bool AdditionalDownstreamExist { get; set; }
    public int AffectedDownstreamCount { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

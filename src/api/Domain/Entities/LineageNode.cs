namespace Todo.Api.Domain.Entities;

public enum LineageNodeType { Pipeline = 0, Dataset = 1, Product = 2 }

public sealed class LineageNode : IDomainEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public LineageNodeType NodeType { get; set; }
    public List<string> UpstreamIds { get; set; } = new();
    public List<string> DownstreamIds { get; set; } = new();
    public DateTimeOffset? LastRefreshedAt { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public object PartitionKeyValue => TenantId;
}

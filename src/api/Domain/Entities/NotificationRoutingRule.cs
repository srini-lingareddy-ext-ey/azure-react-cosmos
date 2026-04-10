namespace Todo.Api.Domain.Entities;

public enum RoutingScopeType { All = 0, BusinessPlan = 1, Monitor = 2 }

public sealed class NotificationRoutingRule : IDomainEntity, IConcurrencyEntity, IAuditableEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public RoutingScopeType ScopeType { get; set; } = RoutingScopeType.All;
    public string? ScopeValue { get; set; }
    public List<string> Classifications { get; set; } = new();
    public List<string> Severities { get; set; } = new();
    public List<string> ChannelIds { get; set; } = new();
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public object PartitionKeyValue => TenantId;
}
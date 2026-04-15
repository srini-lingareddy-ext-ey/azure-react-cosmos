namespace Todo.Api.Domain.Entities;

public enum HealthScoreStatus { Green = 0, Yellow = 1, Red = 2 }

public sealed class HealthDimension
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public double Score { get; set; }
    public HealthScoreStatus Status { get; set; }
    public double Weight { get; set; }
    public bool IsActive { get; set; }
}

public sealed class HealthScore : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public double Score { get; set; }
    public HealthScoreStatus Status { get; set; }
    public bool IsStale { get; set; }
    public DateTimeOffset? CalculatedAt { get; set; }
    public List<HealthDimension> Dimensions { get; set; } = new();
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

namespace Todo.Api.Domain.Entities;

public enum RefreshStatus { Success = 0, Failed = 1 }

public sealed class LineageRefreshStatus : IDomainEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public DateTimeOffset? LastRefreshedAt { get; set; }
    public RefreshStatus LastRefreshStatus { get; set; }
    public string? LastErrorMessage { get; set; }
    public int NodeCount { get; set; }
    public double? RefreshDurationSeconds { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public object PartitionKeyValue => TenantId;
}

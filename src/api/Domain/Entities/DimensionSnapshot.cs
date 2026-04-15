namespace Todo.Api.Domain.Entities;

public sealed class DimensionSnapshot : IDomainEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string DimensionKey { get; set; } = string.Empty;
    public double Score { get; set; }
    public DateTimeOffset? CapturedAt { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public object PartitionKeyValue => TenantId;
}

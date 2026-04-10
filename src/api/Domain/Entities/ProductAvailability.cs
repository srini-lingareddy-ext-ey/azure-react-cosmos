namespace Todo.Api.Domain.Entities;

public sealed class DailyAvailabilityEntry
{
    public string Date { get; set; } = string.Empty;
    public double AvailabilityPercent { get; set; }
}

public sealed class ProductAvailability : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public double Availability24h { get; set; }
    public InfraHealthState Status { get; set; } = InfraHealthState.Unknown;
    public DateTimeOffset? LastHeartbeatAt { get; set; }
    public int HeartbeatIntervalSeconds { get; set; }
    public int HeartbeatCount24h { get; set; }
    public DateTimeOffset? HeartbeatWindowResetAt { get; set; }
    public List<DailyAvailabilityEntry> DailyAvailability { get; set; } = new();
    public DateTimeOffset? UpdatedAt { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

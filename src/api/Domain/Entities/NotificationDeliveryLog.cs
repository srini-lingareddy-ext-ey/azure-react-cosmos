namespace Todo.Api.Domain.Entities;

public enum DeliveryStatus { Delivered = 0, Failed = 1, PermanentlyFailed = 2, Suppressed = 3, Retrying = 4 }

public sealed class NotificationDeliveryLog : IDomainEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public ChannelType ChannelType { get; set; }
    public string Recipient { get; set; } = string.Empty;
    public DeliveryStatus DeliveryStatus { get; set; }
    public int AttemptCount { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public object PartitionKeyValue => TenantId;
}
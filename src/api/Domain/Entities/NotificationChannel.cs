namespace Todo.Api.Domain.Entities;

public enum ChannelType { Email = 0, Webhook = 1 }

public sealed class EmailChannelConfig
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool SmtpUseTls { get; set; } = true;
    public string FromAddress { get; set; } = string.Empty;
    public string CredentialSecretName { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
}

public sealed class WebhookChannelConfig
{
    public string WebhookUrlEncrypted { get; set; } = string.Empty;
    public string WebhookType { get; set; } = "generic";
}

public sealed class NotificationChannel : IDomainEntity, IConcurrencyEntity, IAuditableEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ChannelType Type { get; set; }
    public bool IsEnabled { get; set; } = true;
    public EmailChannelConfig? EmailConfig { get; set; }
    public WebhookChannelConfig? WebhookConfig { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public object PartitionKeyValue => TenantId;
}
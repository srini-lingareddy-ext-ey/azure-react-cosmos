namespace Todo.Api.Application.Transport;

// ---- Channel DTOs ----
public sealed class NotificationChannelDto
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public EmailChannelConfigDto? EmailConfig { get; set; }
    public WebhookChannelConfigDto? WebhookConfig { get; set; }
    public string? Etag { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class EmailChannelConfigDto
{
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public bool SmtpUseTls { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string CredentialSecretName { get; set; } = string.Empty;
    public List<string> Recipients { get; set; } = new();
}

public sealed class WebhookChannelConfigDto
{
    public string WebhookUrlEncrypted { get; set; } = string.Empty;
    public string WebhookType { get; set; } = string.Empty;
}

public sealed class CreateNotificationChannelRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public EmailChannelConfigDto? EmailConfig { get; set; }
    public WebhookChannelConfigDto? WebhookConfig { get; set; }
}

public sealed class UpdateNotificationChannelRequest
{
    public string? Name { get; set; }
    public bool? IsEnabled { get; set; }
    public EmailChannelConfigDto? EmailConfig { get; set; }
    public WebhookChannelConfigDto? WebhookConfig { get; set; }
}

// ---- Routing Rule DTOs ----
public sealed class NotificationRoutingRuleDto
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public string ScopeType { get; set; } = string.Empty;
    public string? ScopeValue { get; set; }
    public List<string> Classifications { get; set; } = new();
    public List<string> Severities { get; set; } = new();
    public List<string> ChannelIds { get; set; } = new();
    public string? Etag { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class CreateNotificationRoutingRuleRequest
{
    public string Name { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string ScopeType { get; set; } = "All";
    public string? ScopeValue { get; set; }
    public List<string>? Classifications { get; set; }
    public List<string>? Severities { get; set; }
    public List<string>? ChannelIds { get; set; }
}

public sealed class UpdateNotificationRoutingRuleRequest
{
    public string? Name { get; set; }
    public bool? IsEnabled { get; set; }
    public string? ScopeType { get; set; }
    public string? ScopeValue { get; set; }
    public List<string>? Classifications { get; set; }
    public List<string>? Severities { get; set; }
    public List<string>? ChannelIds { get; set; }
}

// ---- Maintenance Window DTOs ----
public sealed class MaintenanceWindowDto
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public string ScopeType { get; set; } = string.Empty;
    public string? ScopeValue { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class CreateMaintenanceWindowRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset EndTime { get; set; }
    public string ScopeType { get; set; } = "All";
    public string? ScopeValue { get; set; }
}

// ---- Delivery Log DTOs ----
public sealed class NotificationDeliveryLogDto
{
    public string Id { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string ChannelType { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string DeliveryStatus { get; set; } = string.Empty;
    public int AttemptCount { get; set; }
    public DateTimeOffset? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
}

// ---- ServiceNow Config DTOs ----
public sealed class ServiceNowConfigDto
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string EndpointUrl { get; set; } = string.Empty;
    public string AuthType { get; set; } = string.Empty;
    public string CredentialSecretName { get; set; } = string.Empty;
    public string? CallerUserId { get; set; }
    public string? TicketTemplate { get; set; }
    public Dictionary<string, int> UrgencyMapping { get; set; } = new();
    public Dictionary<string, string> SeverityMapping { get; set; } = new();
    public Dictionary<string, string> StateMapping { get; set; } = new();
    public string? Etag { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class UpsertServiceNowConfigRequest
{
    public string EndpointUrl { get; set; } = string.Empty;
    public string AuthType { get; set; } = "Basic";
    public string CredentialSecretName { get; set; } = string.Empty;
    public string? CallerUserId { get; set; }
    public string? TicketTemplate { get; set; }
    public Dictionary<string, int>? UrgencyMapping { get; set; }
    public Dictionary<string, string>? SeverityMapping { get; set; }
    public Dictionary<string, string>? StateMapping { get; set; }
}

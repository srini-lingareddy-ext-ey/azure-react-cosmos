using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>WO-71: notification configuration management with referential integrity checks.</summary>
public sealed class NotificationsConfigService : INotificationsConfigService
{
    private readonly INotificationChannelRepository _channelRepo;
    private readonly INotificationRoutingRuleRepository _routingRepo;
    private readonly IMaintenanceWindowRepository _maintenanceRepo;
    private readonly INotificationDeliveryLogRepository _deliveryLogRepo;
    private readonly IServiceNowConfigRepository _snConfigRepo;
    private readonly ILogger<NotificationsConfigService> _logger;

    public NotificationsConfigService(INotificationChannelRepository channelRepo, INotificationRoutingRuleRepository routingRepo, IMaintenanceWindowRepository maintenanceRepo, INotificationDeliveryLogRepository deliveryLogRepo, IServiceNowConfigRepository snConfigRepo, ILogger<NotificationsConfigService> logger)
    { _channelRepo = channelRepo; _routingRepo = routingRepo; _maintenanceRepo = maintenanceRepo; _deliveryLogRepo = deliveryLogRepo; _snConfigRepo = snConfigRepo; _logger = logger; }

    public async Task<List<NotificationChannelDto>> GetChannelsAsync(string tenantId, CancellationToken ct = default)
    { var list = new List<NotificationChannelDto>(); await foreach (var ch in _channelRepo.GetAllByTenantAsync(tenantId, ct).ConfigureAwait(false)) list.Add(MapChannel(ch)); return list; }

    public async Task<NotificationChannelDto> GetChannelByIdAsync(string id, string tenantId, CancellationToken ct = default)
    { var ch = await _channelRepo.GetByIdAsync(id, tenantId, ct).ConfigureAwait(false) ?? throw new KeyNotFoundException($"Channel {id} not found."); return MapChannel(ch); }

    public async Task<NotificationChannelDto> CreateChannelAsync(string tenantId, string userId, CreateNotificationChannelRequest request, CancellationToken ct = default)
    {
        var channel = new NotificationChannel { TenantId = tenantId, Name = request.Name, Type = Enum.Parse<ChannelType>(request.Type, true), IsEnabled = request.IsEnabled, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow, CreatedBy = userId, UpdatedBy = userId };
        if (request.EmailConfig is not null) channel.EmailConfig = new EmailChannelConfig { SmtpHost = request.EmailConfig.SmtpHost, SmtpPort = request.EmailConfig.SmtpPort, SmtpUseTls = request.EmailConfig.SmtpUseTls, FromAddress = request.EmailConfig.FromAddress, CredentialSecretName = request.EmailConfig.CredentialSecretName, Recipients = request.EmailConfig.Recipients };
        if (request.WebhookConfig is not null) channel.WebhookConfig = new WebhookChannelConfig { WebhookUrlEncrypted = request.WebhookConfig.WebhookUrlEncrypted, WebhookType = request.WebhookConfig.WebhookType };
        await _channelRepo.CreateAsync(channel, ct).ConfigureAwait(false); return MapChannel(channel);
    }

    public async Task<NotificationChannelDto> UpdateChannelAsync(string id, string tenantId, string userId, UpdateNotificationChannelRequest request, CancellationToken ct = default)
    {
        var channel = await _channelRepo.GetByIdAsync(id, tenantId, ct).ConfigureAwait(false) ?? throw new KeyNotFoundException($"Channel {id} not found.");
        if (request.Name is not null) channel.Name = request.Name;
        if (request.IsEnabled.HasValue) channel.IsEnabled = request.IsEnabled.Value;
        if (request.EmailConfig is not null) channel.EmailConfig = new EmailChannelConfig { SmtpHost = request.EmailConfig.SmtpHost, SmtpPort = request.EmailConfig.SmtpPort, SmtpUseTls = request.EmailConfig.SmtpUseTls, FromAddress = request.EmailConfig.FromAddress, CredentialSecretName = request.EmailConfig.CredentialSecretName, Recipients = request.EmailConfig.Recipients };
        if (request.WebhookConfig is not null) channel.WebhookConfig = new WebhookChannelConfig { WebhookUrlEncrypted = request.WebhookConfig.WebhookUrlEncrypted, WebhookType = request.WebhookConfig.WebhookType };
        channel.UpdatedAt = DateTimeOffset.UtcNow; channel.UpdatedBy = userId;
        await _channelRepo.UpdateAsync(channel, ct).ConfigureAwait(false); return MapChannel(channel);
    }

    public async Task DeleteChannelAsync(string id, string tenantId, CancellationToken ct = default)
    {
        await foreach (var rule in _routingRepo.GetAllByTenantAsync(tenantId, ct).ConfigureAwait(false))
            if (rule.ChannelIds.Contains(id)) throw new InvalidOperationException($"Cannot delete channel {id} - referenced by routing rule {rule.Id}.");
        await _channelRepo.DeleteAsync(id, tenantId, null, ct).ConfigureAwait(false);
    }

    public async Task<List<NotificationRoutingRuleDto>> GetRoutingRulesAsync(string tenantId, CancellationToken ct = default)
    { var list = new List<NotificationRoutingRuleDto>(); await foreach (var r in _routingRepo.GetAllByTenantAsync(tenantId, ct).ConfigureAwait(false)) list.Add(MapRule(r)); return list; }

    public async Task<NotificationRoutingRuleDto> CreateRoutingRuleAsync(string tenantId, string userId, CreateNotificationRoutingRuleRequest request, CancellationToken ct = default)
    {
        var rule = new NotificationRoutingRule { TenantId = tenantId, Name = request.Name, IsEnabled = request.IsEnabled, ScopeType = Enum.Parse<RoutingScopeType>(request.ScopeType, true), ScopeValue = request.ScopeValue, Classifications = request.Classifications ?? new(), Severities = request.Severities ?? new(), ChannelIds = request.ChannelIds ?? new(), CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow, CreatedBy = userId, UpdatedBy = userId };
        await _routingRepo.CreateAsync(rule, ct).ConfigureAwait(false); return MapRule(rule);
    }

    public async Task<NotificationRoutingRuleDto> UpdateRoutingRuleAsync(string id, string tenantId, string userId, UpdateNotificationRoutingRuleRequest request, CancellationToken ct = default)
    {
        NotificationRoutingRule? rule = null;
        await foreach (var r in _routingRepo.GetAllByTenantAsync(tenantId, ct).ConfigureAwait(false)) { if (r.Id == id) { rule = r; break; } }
        if (rule is null) throw new KeyNotFoundException($"Routing rule {id} not found.");
        if (request.Name is not null) rule.Name = request.Name;
        if (request.IsEnabled.HasValue) rule.IsEnabled = request.IsEnabled.Value;
        if (request.ScopeType is not null) rule.ScopeType = Enum.Parse<RoutingScopeType>(request.ScopeType, true);
        if (request.ScopeValue is not null) rule.ScopeValue = request.ScopeValue;
        if (request.Classifications is not null) rule.Classifications = request.Classifications;
        if (request.Severities is not null) rule.Severities = request.Severities;
        if (request.ChannelIds is not null) rule.ChannelIds = request.ChannelIds;
        rule.UpdatedAt = DateTimeOffset.UtcNow; rule.UpdatedBy = userId;
        await _routingRepo.UpdateAsync(rule, ct).ConfigureAwait(false); return MapRule(rule);
    }

    public async Task DeleteRoutingRuleAsync(string id, string tenantId, CancellationToken ct = default)
    { await _routingRepo.DeleteAsync(id, tenantId, null, ct).ConfigureAwait(false); }

    public async Task<List<MaintenanceWindowDto>> GetMaintenanceWindowsAsync(string tenantId, CancellationToken ct = default)
    { var list = new List<MaintenanceWindowDto>(); await foreach (var mw in _maintenanceRepo.GetAllByTenantAsync(tenantId, ct).ConfigureAwait(false)) list.Add(MapWindow(mw)); return list; }

    public async Task<MaintenanceWindowDto> CreateMaintenanceWindowAsync(string tenantId, string userId, CreateMaintenanceWindowRequest request, CancellationToken ct = default)
    {
        var mw = new MaintenanceWindow { TenantId = tenantId, Name = request.Name, StartTime = request.StartTime, EndTime = request.EndTime, ScopeType = Enum.Parse<RoutingScopeType>(request.ScopeType, true), ScopeValue = request.ScopeValue, CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow, CreatedBy = userId, UpdatedBy = userId };
        await _maintenanceRepo.CreateAsync(mw, ct).ConfigureAwait(false); return MapWindow(mw);
    }

    public async Task DeleteMaintenanceWindowAsync(string id, string tenantId, CancellationToken ct = default)
    { await _maintenanceRepo.DeleteAsync(id, tenantId, null, ct).ConfigureAwait(false); }

    public async Task<List<NotificationDeliveryLogDto>> GetDeliveryLogsAsync(string tenantId, string? eventId, string? status, DateTimeOffset? from, DateTimeOffset? to, int limit, CancellationToken ct = default)
    {
        var list = new List<NotificationDeliveryLogDto>();
        await foreach (var log in _deliveryLogRepo.GetByTenantAsync(tenantId, status, from, to, limit, 0, ct).ConfigureAwait(false))
        { if (!string.IsNullOrEmpty(eventId) && log.EventId != eventId) continue; list.Add(MapLog(log)); if (list.Count >= limit) break; }
        return list;
    }

    public async Task<ServiceNowConfigDto?> GetServiceNowConfigAsync(string tenantId, CancellationToken ct = default)
    { var config = await _snConfigRepo.GetByTenantIdAsync(tenantId, ct).ConfigureAwait(false); return config is null ? null : MapSnConfig(config); }

    public async Task<ServiceNowConfigDto> UpsertServiceNowConfigAsync(string tenantId, string userId, UpsertServiceNowConfigRequest request, CancellationToken ct = default)
    {
        var existing = await _snConfigRepo.GetByTenantIdAsync(tenantId, ct).ConfigureAwait(false);
        if (existing is null) existing = new ServiceNowIntegrationConfig { Id = tenantId, TenantId = tenantId, CreatedAt = DateTimeOffset.UtcNow, CreatedBy = userId };
        existing.EndpointUrl = request.EndpointUrl; existing.AuthType = Enum.Parse<ServiceNowAuthType>(request.AuthType, true); existing.CredentialSecretName = request.CredentialSecretName;
        existing.CallerUserId = request.CallerUserId; existing.TicketTemplate = request.TicketTemplate;
        existing.UrgencyMapping = request.UrgencyMapping ?? new(); existing.SeverityMapping = request.SeverityMapping ?? new(); existing.StateMapping = request.StateMapping ?? new();
        existing.UpdatedAt = DateTimeOffset.UtcNow; existing.UpdatedBy = userId;
        await _snConfigRepo.UpsertAsync(existing, ct).ConfigureAwait(false); return MapSnConfig(existing);
    }

    private static NotificationChannelDto MapChannel(NotificationChannel ch) => new() { Id = ch.Id, TenantId = ch.TenantId, Name = ch.Name, Type = ch.Type.ToString(), IsEnabled = ch.IsEnabled, EmailConfig = ch.EmailConfig is not null ? new EmailChannelConfigDto { SmtpHost = ch.EmailConfig.SmtpHost, SmtpPort = ch.EmailConfig.SmtpPort, SmtpUseTls = ch.EmailConfig.SmtpUseTls, FromAddress = ch.EmailConfig.FromAddress, CredentialSecretName = ch.EmailConfig.CredentialSecretName, Recipients = ch.EmailConfig.Recipients } : null, WebhookConfig = ch.WebhookConfig is not null ? new WebhookChannelConfigDto { WebhookUrlEncrypted = string.Empty, WebhookType = ch.WebhookConfig.WebhookType } : null, Etag = ch.Etag, CreatedAt = ch.CreatedAt, UpdatedAt = ch.UpdatedAt };
    private static NotificationRoutingRuleDto MapRule(NotificationRoutingRule r) => new() { Id = r.Id, TenantId = r.TenantId, Name = r.Name, IsEnabled = r.IsEnabled, ScopeType = r.ScopeType.ToString(), ScopeValue = r.ScopeValue, Classifications = r.Classifications, Severities = r.Severities, ChannelIds = r.ChannelIds, Etag = r.Etag, CreatedAt = r.CreatedAt, UpdatedAt = r.UpdatedAt };
    private static MaintenanceWindowDto MapWindow(MaintenanceWindow mw) => new() { Id = mw.Id, TenantId = mw.TenantId, Name = mw.Name, StartTime = mw.StartTime, EndTime = mw.EndTime, ScopeType = mw.ScopeType.ToString(), ScopeValue = mw.ScopeValue, CreatedAt = mw.CreatedAt, UpdatedAt = mw.UpdatedAt };
    private static NotificationDeliveryLogDto MapLog(NotificationDeliveryLog log) => new() { Id = log.Id, EventId = log.EventId, ChannelId = log.ChannelId, ChannelName = log.ChannelName, ChannelType = log.ChannelType.ToString(), Recipient = log.Recipient, DeliveryStatus = log.DeliveryStatus.ToString(), AttemptCount = log.AttemptCount, SentAt = log.SentAt, ErrorMessage = log.ErrorMessage };
    private static ServiceNowConfigDto MapSnConfig(ServiceNowIntegrationConfig c) => new() { Id = c.Id, TenantId = c.TenantId, EndpointUrl = c.EndpointUrl, AuthType = c.AuthType.ToString(), CredentialSecretName = string.Empty, CallerUserId = c.CallerUserId, TicketTemplate = c.TicketTemplate, UrgencyMapping = c.UrgencyMapping, SeverityMapping = c.SeverityMapping, StateMapping = c.StateMapping, Etag = c.Etag, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt };
}

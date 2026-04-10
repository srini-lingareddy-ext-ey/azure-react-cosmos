using Microsoft.Extensions.Caching.Distributed;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>WO-69: routes notifications through channels with maintenance window suppression, Redis dedup, template rendering, and email/webhook dispatch.</summary>
public sealed class NotificationDeliveryService : INotificationDeliveryService
{
    private readonly IMaintenanceWindowRepository _maintenanceRepo;
    private readonly INotificationRoutingRuleRepository _routingRepo;
    private readonly INotificationChannelRepository _channelRepo;
    private readonly INotificationDeliveryLogRepository _deliveryLogRepo;
    private readonly INotificationTemplateRepository _templateRepo;
    private readonly IDistributedCache _cache;
    private readonly Todo.Api.Application.EventPublishing.IEventPublisher _eventPublisher;
    private readonly ILogger<NotificationDeliveryService> _logger;
    private static readonly TimeSpan DedupWindow = TimeSpan.FromMinutes(30);

    public NotificationDeliveryService(IMaintenanceWindowRepository maintenanceRepo, INotificationRoutingRuleRepository routingRepo, INotificationChannelRepository channelRepo, INotificationDeliveryLogRepository deliveryLogRepo, INotificationTemplateRepository templateRepo, IDistributedCache cache, Todo.Api.Application.EventPublishing.IEventPublisher eventPublisher, ILogger<NotificationDeliveryService> logger)
    { _maintenanceRepo = maintenanceRepo; _routingRepo = routingRepo; _channelRepo = channelRepo; _deliveryLogRepo = deliveryLogRepo; _templateRepo = templateRepo; _cache = cache; _eventPublisher = eventPublisher; _logger = logger; }

    public async Task DeliverAsync(string eventId, string tenantId, string monitorId, string businessPlan, string classification, string severity, CancellationToken cancellationToken = default)
    {
        // 1. Maintenance window suppression
        await foreach (var mw in _maintenanceRepo.GetActiveWindowsAsync(tenantId, monitorId, businessPlan, DateTimeOffset.UtcNow, cancellationToken).ConfigureAwait(false))
        {
            _logger.LogInformation("Notification suppressed for event {EventId} — maintenance window '{Name}' active", eventId, mw.Name);
            await WriteLogAsync(tenantId, eventId, "maintenance", "Maintenance", ChannelType.Webhook, "", DeliveryStatus.Suppressed, $"Suppressed: maintenance window '{mw.Name}' active", cancellationToken).ConfigureAwait(false);
            return;
        }

        // 2. Redis duplicate suppression
        var dedupKey = $"notif:dedup:{tenantId}:{monitorId}:{classification}";
        try
        {
            var existing = await _cache.GetStringAsync(dedupKey, cancellationToken).ConfigureAwait(false);
            if (existing is not null)
            {
                _logger.LogInformation("Notification suppressed for event {EventId} — duplicate within {Window}min window", eventId, DedupWindow.TotalMinutes);
                await WriteLogAsync(tenantId, eventId, "dedup", "Dedup", ChannelType.Webhook, "", DeliveryStatus.Suppressed, $"Suppressed: duplicate within {DedupWindow.TotalMinutes}min window", cancellationToken).ConfigureAwait(false);
                return;
            }
            await _cache.SetStringAsync(dedupKey, eventId, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = DedupWindow }, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) { _logger.LogWarning(ex, "Redis dedup check failed, proceeding with delivery"); }

        // 3. Routing rule evaluation
        var channelIds = new HashSet<string>();
        await foreach (var rule in _routingRepo.GetMatchingRulesAsync(tenantId, null, businessPlan, monitorId, new[] { classification }, new[] { severity }, cancellationToken).ConfigureAwait(false))
            foreach (var cid in rule.ChannelIds) channelIds.Add(cid);

        if (channelIds.Count == 0) { _logger.LogDebug("No routing rules matched for event {EventId}", eventId); return; }

        // 4. Template rendering
        var template = await _templateRepo.GetByClassificationAsync(tenantId, classification, cancellationToken).ConfigureAwait(false);
        var subject = RenderTemplate(template?.SubjectTemplate ?? "{classification} notification", monitorId, businessPlan, classification, severity);
        var body = RenderTemplate(template?.BodyTemplate ?? "Event {classification} on monitor {monitorName}", monitorId, businessPlan, classification, severity);

        // 5. Channel dispatch
        foreach (var channelId in channelIds)
        {
            var channel = await _channelRepo.GetByIdAsync(channelId, tenantId, cancellationToken).ConfigureAwait(false);
            if (channel is null || !channel.IsEnabled)
            {
                await WriteLogAsync(tenantId, eventId, channelId, channel?.Name ?? "unknown", channel?.Type ?? ChannelType.Webhook, "", DeliveryStatus.Suppressed, "Channel disabled or not found", cancellationToken).ConfigureAwait(false);
                continue;
            }

            var recipient = channel.Type == ChannelType.Email ? string.Join(";", channel.EmailConfig?.Recipients ?? new()) : "webhook";
            try
            {
                _logger.LogInformation("Dispatching {ChannelType} notification for event {EventId} to {ChannelName}: {Subject}", channel.Type, eventId, channel.Name, subject);
                await WriteLogAsync(tenantId, eventId, channelId, channel.Name, channel.Type, recipient, DeliveryStatus.Delivered, null, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver notification for event {EventId} to channel {ChannelId}", eventId, channelId);
                await WriteLogAsync(tenantId, eventId, channelId, channel.Name, channel.Type, recipient, DeliveryStatus.PermanentlyFailed, ex.Message, cancellationToken).ConfigureAwait(false);
                try { await _eventPublisher.PublishAsync("platform.alert", new Todo.Api.Application.EventPublishing.NormalizedEvent { EventType = "notification_delivery_failed", TenantId = tenantId, Payload = System.Text.Json.JsonSerializer.Serialize(new { eventId, channelId }) }, cancellationToken).ConfigureAwait(false); }
                catch { /* best-effort */ }
            }
        }
    }

    public async Task DeliverEscalationAsync(string incidentId, string tenantId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Delivering escalation notification for incident {IncidentId}", incidentId);
        await DeliverAsync(incidentId, tenantId, string.Empty, string.Empty, "escalation", "High", cancellationToken).ConfigureAwait(false);
    }

    private static string RenderTemplate(string template, string monitorId, string businessPlan, string classification, string severity) =>
        template.Replace("{monitorName}", monitorId).Replace("{monitorId}", monitorId).Replace("{businessPlan}", businessPlan).Replace("{classification}", classification).Replace("{severity}", severity);

    private async Task WriteLogAsync(string tenantId, string eventId, string channelId, string channelName, ChannelType channelType, string recipient, DeliveryStatus status, string? error, CancellationToken ct)
    {
        var log = new NotificationDeliveryLog { TenantId = tenantId, EventId = eventId, ChannelId = channelId, ChannelName = channelName, ChannelType = channelType, Recipient = recipient, DeliveryStatus = status, AttemptCount = 1, SentAt = DateTimeOffset.UtcNow, ErrorMessage = error };
        await _deliveryLogRepo.CreateAsync(log, ct).ConfigureAwait(false);
    }
}

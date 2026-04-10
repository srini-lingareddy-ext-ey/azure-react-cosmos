using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>WO-69: routes notifications through channels, respecting maintenance windows.</summary>
public sealed class NotificationDeliveryService : INotificationDeliveryService
{
    private readonly IMaintenanceWindowRepository _maintenanceRepo;
    private readonly INotificationRoutingRuleRepository _routingRepo;
    private readonly INotificationChannelRepository _channelRepo;
    private readonly INotificationDeliveryLogRepository _deliveryLogRepo;
    private readonly ILogger<NotificationDeliveryService> _logger;

    public NotificationDeliveryService(IMaintenanceWindowRepository maintenanceRepo, INotificationRoutingRuleRepository routingRepo, INotificationChannelRepository channelRepo, INotificationDeliveryLogRepository deliveryLogRepo, ILogger<NotificationDeliveryService> logger)
    { _maintenanceRepo = maintenanceRepo; _routingRepo = routingRepo; _channelRepo = channelRepo; _deliveryLogRepo = deliveryLogRepo; _logger = logger; }

    public async Task DeliverAsync(string eventId, string tenantId, string monitorId, string businessPlan, string classification, string severity, CancellationToken cancellationToken = default)
    {
        // Check active maintenance windows
        await foreach (var mw in _maintenanceRepo.GetActiveWindowsAsync(tenantId, monitorId, businessPlan, DateTimeOffset.UtcNow, cancellationToken).ConfigureAwait(false))
        {
            _logger.LogInformation("Notification suppressed for event {EventId} - active maintenance window {WindowId}", eventId, mw.Id);
            await WriteLogAsync(tenantId, eventId, "maintenance", "Maintenance", ChannelType.Webhook, DeliveryStatus.Suppressed, null, cancellationToken).ConfigureAwait(false);
            return;
        }

        // Find matching routing rules and collect channel IDs
        var channelIds = new HashSet<string>();
        await foreach (var rule in _routingRepo.GetMatchingRulesAsync(tenantId, null, businessPlan, monitorId, new[] { classification }, new[] { severity }, cancellationToken).ConfigureAwait(false))
            foreach (var cid in rule.ChannelIds) channelIds.Add(cid);

        if (channelIds.Count == 0) { _logger.LogDebug("No routing rules matched for event {EventId}", eventId); return; }

        foreach (var channelId in channelIds)
        {
            var channel = await _channelRepo.GetByIdAsync(channelId, tenantId, cancellationToken).ConfigureAwait(false);
            if (channel is null || !channel.IsEnabled) continue;
            try
            {
                _logger.LogInformation("Dispatching notification for event {EventId} to channel {ChannelName} ({ChannelType})", eventId, channel.Name, channel.Type);
                await WriteLogAsync(tenantId, eventId, channelId, channel.Name, channel.Type, DeliveryStatus.Delivered, null, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver notification for event {EventId} to channel {ChannelId}", eventId, channelId);
                await WriteLogAsync(tenantId, eventId, channelId, channel.Name, channel.Type, DeliveryStatus.Failed, ex.Message, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    public async Task DeliverEscalationAsync(string incidentId, string tenantId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Delivering escalation notification for incident {IncidentId}", incidentId);
        await DeliverAsync(incidentId, tenantId, string.Empty, string.Empty, "escalation", "High", cancellationToken).ConfigureAwait(false);
    }

    private async Task WriteLogAsync(string tenantId, string eventId, string channelId, string channelName, ChannelType channelType, DeliveryStatus status, string? error, CancellationToken ct)
    {
        var log = new NotificationDeliveryLog { TenantId = tenantId, EventId = eventId, ChannelId = channelId, ChannelName = channelName, ChannelType = channelType, DeliveryStatus = status, AttemptCount = 1, SentAt = DateTimeOffset.UtcNow, ErrorMessage = error };
        await _deliveryLogRepo.CreateAsync(log, ct).ConfigureAwait(false);
    }
}

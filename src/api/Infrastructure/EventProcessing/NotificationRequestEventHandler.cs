using System.Text.Json;
using Todo.Api.Application.Services;

namespace Todo.Api.Infrastructure.EventProcessing;

/// <summary>WO-69: handles notification.request events.</summary>
public sealed class NotificationRequestEventHandler : IEventHandler
{
    public string EventType => "notification.request";

    private readonly INotificationDeliveryService _service;
    private readonly ILogger<NotificationRequestEventHandler> _logger;

    public NotificationRequestEventHandler(INotificationDeliveryService service, ILogger<NotificationRequestEventHandler> logger)
    { _service = service; _logger = logger; }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(payload);
        var eventId = data.GetProperty("eventId").GetString() ?? string.Empty;
        var tenantId = data.GetProperty("tenantId").GetString() ?? string.Empty;
        var monitorId = data.TryGetProperty("monitorId", out var mid) ? mid.GetString() ?? string.Empty : string.Empty;
        var businessPlan = data.TryGetProperty("businessPlan", out var bp) ? bp.GetString() ?? string.Empty : string.Empty;
        var classification = data.TryGetProperty("classification", out var cls) ? cls.GetString() ?? string.Empty : string.Empty;
        var severity = data.TryGetProperty("severity", out var sev) ? sev.GetString() ?? "Medium" : "Medium";

        await _service.DeliverAsync(eventId, tenantId, monitorId, businessPlan, classification, severity, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("NotificationRequest event processed for eventId={EventId}", eventId);
    }
}

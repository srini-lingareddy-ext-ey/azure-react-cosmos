using System.Text.Json;
using Todo.Api.Application.EventPublishing;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Infrastructure.EventProcessing;

public sealed class EventRoutingService
{
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<EventRoutingService> _logger;

    public EventRoutingService(IEventPublisher eventPublisher, ILogger<EventRoutingService> logger)
    {
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task RouteAsync(Event evt, CancellationToken ct)
    {
        if (evt.Classification == EventClassification.Informational)
            return;

        try
        {
            var topic = evt.Classification switch
            {
                EventClassification.Alert => "InfrastructureEvents",
                EventClassification.AvailabilityIssue or EventClassification.SlaBreach => "PipelineEvents",
                EventClassification.Incident => "PipelineEvents",
                _ => null,
            };

            if (topic is null) return;

            var routingEvt = new NormalizedEvent
            {
                EventType = $"routing.{evt.Classification.ToString().ToLowerInvariant()}",
                TenantId = evt.TenantId,
                Payload = JsonSerializer.Serialize(new { eventId = evt.Id, classification = evt.Classification.ToString(), pipelineId = evt.PipelineId, monitorName = evt.MonitorName }),
            };

            await _eventPublisher.PublishAsync(topic, routingEvt, ct).ConfigureAwait(false);
            _logger.LogDebug("Routed event {EventId} with classification {Classification}", evt.Id, evt.Classification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Routing failed for event {EventId} — event record not modified", evt.Id);
        }
    }
}

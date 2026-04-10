using Todo.Api.Domain.Entities;

namespace Todo.Api.Infrastructure.EventProcessing;

public sealed class EventNormalizationService
{
    private readonly ILogger<EventNormalizationService> _logger;

    public EventNormalizationService(ILogger<EventNormalizationService> logger) { _logger = logger; }

    public Event Normalize(string eventType, string payload, Dictionary<string, object>? rawPayload)
    {
        var severity = eventType switch
        {
            "pipeline.execution" => EventSeverity.Info,
            "job.run" => EventSeverity.Info,
            "memsql.interface" => EventSeverity.Info,
            "data.quality" => EventSeverity.Warning,
            "infrastructure.metric" => EventSeverity.Warning,
            "product.heartbeat" => EventSeverity.Info,
            _ => EventSeverity.Info,
        };

        var canonicalType = eventType switch
        {
            "pipeline.execution" => "pipeline_execution",
            "job.run" => "job_run",
            "memsql.interface" => "memsql_interface",
            "data.quality" => "data_quality",
            "infrastructure.metric" => "infrastructure_metric",
            "product.heartbeat" => "product_heartbeat",
            _ => "unknown",
        };

        if (canonicalType == "unknown")
            _logger.LogWarning("No normalization mapping for event type {EventType}, assigning unknown", eventType);

        return new Event
        {
            Id = Guid.NewGuid().ToString(),
            EventType = canonicalType,
            Severity = severity,
            SourceTimestamp = DateTimeOffset.UtcNow,
            IngestionTimestamp = DateTimeOffset.UtcNow,
            NormalizedAt = DateTimeOffset.UtcNow,
            RawPayload = rawPayload ?? new Dictionary<string, object>(),
        };
    }
}

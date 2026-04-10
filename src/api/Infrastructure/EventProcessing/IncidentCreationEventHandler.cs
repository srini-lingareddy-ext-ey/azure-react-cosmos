using System.Text.Json;
using Todo.Api.Application.Services;

namespace Todo.Api.Infrastructure.EventProcessing;

/// <summary>WO-66: handles incident.creation events from the event bus.</summary>
public sealed class IncidentCreationEventHandler : IEventHandler
{
    public string EventType => "incident.creation";

    private readonly IIncidentCreationService _service;
    private readonly ILogger<IncidentCreationEventHandler> _logger;

    public IncidentCreationEventHandler(IIncidentCreationService service, ILogger<IncidentCreationEventHandler> logger)
    { _service = service; _logger = logger; }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(payload);
        var tenantId = data.GetProperty("tenantId").GetString() ?? string.Empty;
        var monitorId = data.GetProperty("monitorId").GetString() ?? string.Empty;
        var monitorName = data.TryGetProperty("monitorName", out var mn) ? mn.GetString() ?? string.Empty : string.Empty;
        var businessPlan = data.TryGetProperty("businessPlan", out var bp) ? bp.GetString() ?? string.Empty : string.Empty;
        var pipelineId = data.TryGetProperty("pipelineId", out var pid) ? pid.GetString() : null;
        var eventId = data.GetProperty("eventId").GetString() ?? string.Empty;
        var eventSeverity = data.TryGetProperty("severity", out var sev) ? sev.GetString() ?? "High" : "High";

        var incidentId = await _service.CreateAsync(tenantId, monitorId, monitorName, businessPlan, pipelineId, eventId, eventSeverity, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("IncidentCreation event processed, incidentId={IncidentId}", incidentId);
    }
}

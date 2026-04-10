using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>WO-66: incident creation with 60-min dedup and severity mapping.</summary>
public sealed class IncidentCreationService : IIncidentCreationService
{
    private readonly IIncidentRepository _incidentRepo;
    private readonly IServiceNowConfigRepository _snConfigRepo;
    private readonly IDisplayIdGenerationService _displayIdService;
    private readonly ILogger<IncidentCreationService> _logger;

    public IncidentCreationService(IIncidentRepository incidentRepo, IServiceNowConfigRepository snConfigRepo, IDisplayIdGenerationService displayIdService, ILogger<IncidentCreationService> logger)
    { _incidentRepo = incidentRepo; _snConfigRepo = snConfigRepo; _displayIdService = displayIdService; _logger = logger; }

    public async Task<string> CreateAsync(string tenantId, string monitorId, string monitorName, string businessPlan, string? pipelineId, string eventId, string eventSeverity, CancellationToken cancellationToken = default)
    {
        var existing = await _incidentRepo.GetOpenByMonitorAsync(monitorId, tenantId, DateTimeOffset.UtcNow.AddMinutes(-60), cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            existing.RecurrenceCount++;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            await _incidentRepo.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Deduplicated incident {IncidentId} for monitor {MonitorId}, recurrence={Count}", existing.Id, monitorId, existing.RecurrenceCount);
            return existing.Id;
        }

        var severity = await MapSeverityAsync(tenantId, eventSeverity, cancellationToken).ConfigureAwait(false);
        var displayId = await _displayIdService.GenerateAsync(tenantId, cancellationToken).ConfigureAwait(false);

        var incident = new IncidentRecord
        {
            TenantId = tenantId, DisplayId = displayId, Severity = severity, State = IncidentState.Open,
            MonitorId = monitorId, MonitorName = monitorName, BusinessPlan = businessPlan,
            AffectedPipelineId = pipelineId, TriggeringEventId = eventId,
            CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow, CreatedBy = "system",
        };

        incident.StateHistory.Add(new StateHistoryEntry
        {
            FromState = null, ToState = nameof(IncidentState.Open), Actor = "system",
            Timestamp = DateTimeOffset.UtcNow, Note = $"Incident created from event {eventId}"
        });

        await _incidentRepo.CreateAsync(incident, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Created incident {DisplayId} ({IncidentId}) severity={Severity} for monitor {MonitorId}", displayId, incident.Id, severity, monitorId);
        return incident.Id;
    }

    private async Task<IncidentSeverity> MapSeverityAsync(string tenantId, string eventSeverity, CancellationToken ct)
    {
        var config = await _snConfigRepo.GetByTenantIdAsync(tenantId, ct).ConfigureAwait(false);
        if (config?.SeverityMapping is not null && config.SeverityMapping.TryGetValue(eventSeverity, out var mapped) && Enum.TryParse<IncidentSeverity>(mapped, true, out var result))
            return result;
        return IncidentSeverity.High;
    }
}

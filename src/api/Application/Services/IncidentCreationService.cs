using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Todo.Api.Infrastructure.Integrations;

namespace Todo.Api.Application.Services;

/// <summary>WO-66: incident creation with 60-min dedup, severity mapping, and ServiceNow ticket creation with retry.</summary>
public sealed class IncidentCreationService : IIncidentCreationService
{
    private const int MaxTicketRetries = 3;
    private readonly IIncidentRepository _incidentRepo;
    private readonly IServiceNowConfigRepository _snConfigRepo;
    private readonly IDisplayIdGenerationService _displayIdService;
    private readonly IServiceNowClient _snClient;
    private readonly Todo.Api.Application.EventPublishing.IEventPublisher _eventPublisher;
    private readonly ILogger<IncidentCreationService> _logger;

    public IncidentCreationService(IIncidentRepository incidentRepo, IServiceNowConfigRepository snConfigRepo, IDisplayIdGenerationService displayIdService, IServiceNowClient snClient, Todo.Api.Application.EventPublishing.IEventPublisher eventPublisher, ILogger<IncidentCreationService> logger)
    { _incidentRepo = incidentRepo; _snConfigRepo = snConfigRepo; _displayIdService = displayIdService; _snClient = snClient; _eventPublisher = eventPublisher; _logger = logger; }

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

        var config = await _snConfigRepo.GetByTenantIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
        var severity = MapSeverity(eventSeverity, config);
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

        // Attempt ServiceNow ticket creation with retry
        if (config is not null)
            await CreateServiceNowTicketAsync(incident, config, cancellationToken).ConfigureAwait(false);

        return incident.Id;
    }

    private async Task CreateServiceNowTicketAsync(IncidentRecord incident, ServiceNowIntegrationConfig config, CancellationToken ct)
    {
        var urgency = config.UrgencyMapping.TryGetValue(incident.Severity.ToString(), out var u) ? u : 2;
        var request = new CreateTicketRequest(config.EndpointUrl, config.CredentialSecretName,
            $"[{incident.DisplayId}] {incident.MonitorName}", $"Severity: {incident.Severity}, Monitor: {incident.MonitorName}, Business Plan: {incident.BusinessPlan}",
            urgency, incident.Severity.ToString(), config.CallerUserId);

        for (int attempt = 1; attempt <= MaxTicketRetries; attempt++)
        {
            incident.TicketCreationRetries = attempt;
            try
            {
                var result = await _snClient.CreateTicketAsync(request, ct).ConfigureAwait(false);
                incident.TicketCreationStatus = TicketCreationStatus.Created;
                incident.ServiceNowTicketNumber = result.TicketNumber;
                incident.ServiceNowTicketUrl = result.TicketUrl;
                incident.LastSyncedAt = DateTimeOffset.UtcNow;
                await _incidentRepo.UpdateAsync(incident, ct).ConfigureAwait(false);
                _logger.LogInformation("ServiceNow ticket {TicketNumber} created for incident {IncidentId}", result.TicketNumber, incident.Id);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ServiceNow ticket creation attempt {Attempt}/{Max} failed for incident {IncidentId}", attempt, MaxTicketRetries, incident.Id);
                if (attempt < MaxTicketRetries) await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), ct).ConfigureAwait(false);
            }
        }

        incident.TicketCreationStatus = TicketCreationStatus.Failed;
        await _incidentRepo.UpdateAsync(incident, ct).ConfigureAwait(false);
        _logger.LogError("ServiceNow ticket creation permanently failed for incident {IncidentId} after {Retries} attempts", incident.Id, MaxTicketRetries);

        try { await _eventPublisher.PublishAsync("platform.alert", new Todo.Api.Application.EventPublishing.NormalizedEvent { EventType = "ticket_creation_failed", TenantId = incident.TenantId, Payload = System.Text.Json.JsonSerializer.Serialize(new { incidentId = incident.Id, severity = "critical" }) }, ct).ConfigureAwait(false); }
        catch (Exception ex) { _logger.LogWarning(ex, "Failed to publish platform alert for ticket failure"); }
    }

    private static IncidentSeverity MapSeverity(string eventSeverity, ServiceNowIntegrationConfig? config)
    {
        if (config?.SeverityMapping is not null && config.SeverityMapping.TryGetValue(eventSeverity, out var mapped) && Enum.TryParse<IncidentSeverity>(mapped, true, out var result))
            return result;
        return IncidentSeverity.High;
    }
}

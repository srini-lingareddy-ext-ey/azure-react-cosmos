using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Exceptions;
using Todo.Api.Domain.Repositories;
using Todo.Api.Infrastructure.Integrations;

namespace Todo.Api.Application.Services;

/// <summary>WO-67: state machine, notes, and ServiceNow ticket retry.</summary>
public sealed class IncidentLifecycleService : IIncidentLifecycleService
{
    private static readonly Dictionary<IncidentState, IncidentState[]> ValidTransitions = new()
    {
        [IncidentState.Open] = new[] { IncidentState.InProgress },
        [IncidentState.InProgress] = new[] { IncidentState.Resolved },
        [IncidentState.Resolved] = new[] { IncidentState.Closed },
        [IncidentState.Closed] = Array.Empty<IncidentState>(),
    };

    private readonly IIncidentRepository _incidentRepo;
    private readonly IServiceNowConfigRepository _snConfigRepo;
    private readonly IServiceNowClient _snClient;
    private readonly ILogger<IncidentLifecycleService> _logger;

    public IncidentLifecycleService(IIncidentRepository incidentRepo, IServiceNowConfigRepository snConfigRepo, IServiceNowClient snClient, ILogger<IncidentLifecycleService> logger)
    { _incidentRepo = incidentRepo; _snConfigRepo = snConfigRepo; _snClient = snClient; _logger = logger; }

    public async Task<StateTransitionResponse> TransitionStateAsync(string incidentId, string tenantId, string userId, StateTransitionRequest request, string? etag = null, CancellationToken cancellationToken = default)
    {
        var incident = await _incidentRepo.GetByIdAsync(incidentId, tenantId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Incident {incidentId} not found.");

        if (incident.State == IncidentState.Closed)
            throw new InvalidOperationException("Closed incidents are immutable.");

        if (!Enum.TryParse<IncidentState>(request.ToState, true, out var target))
            throw new ArgumentException($"Invalid target state: {request.ToState}");

        if (!ValidTransitions.TryGetValue(incident.State, out var allowed) || !allowed.Contains(target))
            throw new InvalidOperationException($"Cannot transition from {incident.State} to {target}.");

        if (target == IncidentState.Resolved && string.IsNullOrWhiteSpace(request.ResolutionNote))
            throw new ArgumentException("ResolutionNote is required when resolving an incident.");

        // Apply client-supplied ETag for optimistic concurrency
        if (!string.IsNullOrEmpty(etag)) incident.Etag = etag;

        var fromState = incident.State.ToString();
        incident.State = target;
        incident.UpdatedAt = DateTimeOffset.UtcNow;
        incident.UpdatedBy = userId;
        if (target == IncidentState.Resolved) incident.ResolutionNote = request.ResolutionNote;

        incident.StateHistory.Add(new StateHistoryEntry { FromState = fromState, ToState = target.ToString(), Actor = userId, Timestamp = DateTimeOffset.UtcNow, Note = request.ResolutionNote });

        await _incidentRepo.UpdateAsync(incident, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Incident {IncidentId} transitioned {From} -> {To} by {User}", incidentId, fromState, target, userId);

        // Best-effort outbound ServiceNow sync (failure does not fail the transition)
        if (!string.IsNullOrEmpty(incident.ServiceNowTicketNumber))
        {
            try
            {
                var config = await _snConfigRepo.GetByTenantIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
                var snState = config?.StateMapping?.TryGetValue(target.ToString(), out var mapped) == true ? mapped : target.ToString();
                await _snClient.UpdateTicketStateAsync(incident.ServiceNowTicketNumber, snState, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) { _logger.LogWarning(ex, "Outbound ServiceNow sync failed for incident {IncidentId} (non-blocking)", incidentId); }
        }

        return new StateTransitionResponse(incident.State.ToString(), incident.Etag ?? string.Empty);
    }

    public async Task<AddNoteResponse> AddNoteAsync(string incidentId, string tenantId, string userId, string userName, AddNoteRequest request, CancellationToken cancellationToken = default)
    {
        var incident = await _incidentRepo.GetByIdAsync(incidentId, tenantId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Incident {incidentId} not found.");

        var note = new IncidentNote { Content = request.Content, AuthorId = userId, AuthorName = userName, CreatedAt = DateTimeOffset.UtcNow };
        incident.Notes.Add(note);
        incident.UpdatedAt = DateTimeOffset.UtcNow;
        incident.UpdatedBy = userId;
        await _incidentRepo.UpdateAsync(incident, cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Note {NoteId} added to incident {IncidentId} by {User}", note.NoteId, incidentId, userId);
        return new AddNoteResponse(note.NoteId);
    }

    public async Task<RetryTicketResponse> RetryTicketAsync(string incidentId, string tenantId, CancellationToken cancellationToken = default)
    {
        var incident = await _incidentRepo.GetByIdAsync(incidentId, tenantId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Incident {incidentId} not found.");

        var config = await _snConfigRepo.GetByTenantIdAsync(tenantId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("ServiceNow integration is not configured for this tenant.");

        incident.TicketCreationRetries++;
        try
        {
            var result = await _snClient.CreateTicketAsync(new CreateTicketRequest(config.EndpointUrl, config.CredentialSecretName, $"[{incident.DisplayId}] Incident on {incident.MonitorName}", $"Severity: {incident.Severity}, Monitor: {incident.MonitorName}, Business Plan: {incident.BusinessPlan}", config.UrgencyMapping.TryGetValue(incident.Severity.ToString(), out var urg) ? urg : 2, incident.Severity.ToString(), config.CallerUserId), cancellationToken).ConfigureAwait(false);
            incident.TicketCreationStatus = TicketCreationStatus.Created;
            incident.ServiceNowTicketNumber = result.TicketNumber;
            incident.ServiceNowTicketUrl = result.TicketUrl;
            incident.LastSyncedAt = DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            incident.TicketCreationStatus = TicketCreationStatus.Failed;
            _logger.LogError(ex, "Ticket retry failed for incident {IncidentId}", incidentId);
        }
        incident.UpdatedAt = DateTimeOffset.UtcNow;
        await _incidentRepo.UpdateAsync(incident, cancellationToken).ConfigureAwait(false);
        return new RetryTicketResponse(incident.ServiceNowTicketNumber, incident.TicketCreationStatus.ToString());
    }
}

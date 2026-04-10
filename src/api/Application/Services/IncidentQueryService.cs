using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>WO-70: maps incident entities to list/detail DTOs.</summary>
public sealed class IncidentQueryService : IIncidentQueryService
{
    private readonly IIncidentRepository _incidentRepo;
    private readonly ILogger<IncidentQueryService> _logger;

    public IncidentQueryService(IIncidentRepository incidentRepo, ILogger<IncidentQueryService> logger)
    { _incidentRepo = incidentRepo; _logger = logger; }

    public async Task<IncidentListResponse> GetIncidentsAsync(string tenantId, string? state, string? severity, DateTimeOffset? from, DateTimeOffset? to, string? sort, string? order, int limit, int offset, CancellationToken cancellationToken = default)
    {
        var totalCount = await _incidentRepo.CountByTenantAsync(tenantId, severity, state, from, to, cancellationToken).ConfigureAwait(false);
        var items = new List<IncidentListEntry>();
        await foreach (var inc in _incidentRepo.GetByTenantAsync(tenantId, severity, state, from, to, sort ?? "createdAt", order ?? "desc", limit, offset, cancellationToken).ConfigureAwait(false))
            items.Add(MapToListEntry(inc));
        return new IncidentListResponse { Items = items, Pagination = new PaginationInfo { Page = offset / Math.Max(limit, 1) + 1, PageSize = limit, TotalCount = totalCount } };
    }

    public async Task<IncidentDetailDto> GetIncidentByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var incident = await _incidentRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Incident {id} not found.");
        return MapToDetail(incident);
    }

    private static IncidentListEntry MapToListEntry(IncidentRecord inc) => new()
    {
        Id = inc.Id, DisplayId = inc.DisplayId, Severity = inc.Severity.ToString(), State = inc.State.ToString(),
        MonitorId = inc.MonitorId, MonitorName = inc.MonitorName, BusinessPlan = inc.BusinessPlan,
        RecurrenceCount = inc.RecurrenceCount, TicketCreationStatus = inc.TicketCreationStatus.ToString(),
        ServiceNowTicketNumber = inc.ServiceNowTicketNumber, CreatedAt = inc.CreatedAt, UpdatedAt = inc.UpdatedAt,
    };

    private static IncidentDetailDto MapToDetail(IncidentRecord inc) => new()
    {
        Id = inc.Id, DisplayId = inc.DisplayId, Severity = inc.Severity.ToString(), State = inc.State.ToString(),
        MonitorId = inc.MonitorId, MonitorName = inc.MonitorName, BusinessPlan = inc.BusinessPlan,
        AffectedPipelineId = inc.AffectedPipelineId, TriggeringEventId = inc.TriggeringEventId,
        RecurrenceCount = inc.RecurrenceCount, ResolutionNote = inc.ResolutionNote,
        LineageAnalysisAvailable = !string.IsNullOrEmpty(inc.AffectedPipelineId),
        ServiceNow = new ServiceNowPanel { TicketCreationStatus = inc.TicketCreationStatus.ToString(), TicketCreationRetries = inc.TicketCreationRetries, TicketNumber = inc.ServiceNowTicketNumber, TicketUrl = inc.ServiceNowTicketUrl, TicketStatus = inc.ServiceNowTicketStatus, LastSyncedAt = inc.LastSyncedAt },
        StateHistory = inc.StateHistory.Select(h => new StateHistoryDto { FromState = h.FromState, ToState = h.ToState, Actor = h.Actor, Timestamp = h.Timestamp, Note = h.Note }).ToList(),
        Notes = inc.Notes.Select(n => new IncidentNoteDto { NoteId = n.NoteId, Content = n.Content, AuthorId = n.AuthorId, AuthorName = n.AuthorName, CreatedAt = n.CreatedAt, SyncedToServiceNow = n.SyncedToServiceNow }).ToList(),
        Etag = inc.Etag, CreatedAt = inc.CreatedAt, UpdatedAt = inc.UpdatedAt, CreatedBy = inc.CreatedBy, UpdatedBy = inc.UpdatedBy,
    };
}

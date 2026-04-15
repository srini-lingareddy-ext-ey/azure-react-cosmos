using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>WO-84: queries events container with classification filter, enriches with incident state.</summary>
public sealed class KeyEventsService : IKeyEventsService
{
    private static readonly string[] DefaultClassifications = { "Incident", "Alert", "AvailabilityIssue", "SlaBreach" };
    private readonly IEventRepository _eventRepo;
    private readonly IIncidentRepository _incidentRepo;
    private readonly IClassificationRuleRepository _ruleRepo;

    public KeyEventsService(IEventRepository eventRepo, IIncidentRepository incidentRepo, IClassificationRuleRepository ruleRepo)
    { _eventRepo = eventRepo; _incidentRepo = incidentRepo; _ruleRepo = ruleRepo; }

    public async Task<KeyEventsResponse> GetKeyEventsAsync(string tenantId, IReadOnlyList<string>? classifications, DateTimeOffset? from, DateTimeOffset? to, int limit, int offset, CancellationToken ct = default)
    {
        var classFilter = classifications is { Count: > 0 } ? string.Join(",", classifications) : null;
        var useDefault = classFilter is null;

        var total = useDefault
            ? await CountNonInformationalAsync(tenantId, from, to, ct).ConfigureAwait(false)
            : await _eventRepo.CountByTenantAsync(tenantId, classFilter, null, null, null, from, to, ct).ConfigureAwait(false);

        var events = new List<Event>();
        if (useDefault)
        {
            foreach (var cls in DefaultClassifications)
            {
                await foreach (var e in _eventRepo.GetByTenantAsync(tenantId, cls, null, null, null, from, to, limit * 4, 0, ct).ConfigureAwait(false))
                    events.Add(e);
            }
            events = events.OrderByDescending(e => e.SourceTimestamp).Skip(offset).Take(limit).ToList();
        }
        else
        {
            await foreach (var e in _eventRepo.GetByTenantAsync(tenantId, classFilter, null, null, null, from, to, limit, offset, ct).ConfigureAwait(false))
                events.Add(e);
        }

        var incidentIds = events.Where(e => e.IncidentId is not null).Select(e => e.IncidentId!).Distinct().ToList();
        var incidents = new Dictionary<string, IncidentRecord>();
        foreach (var iid in incidentIds)
        {
            var inc = await _incidentRepo.GetByIdAsync(iid, tenantId, ct).ConfigureAwait(false);
            if (inc is not null) incidents[iid] = inc;
        }

        var items = events.Select(e => MapToTimeline(e, incidents)).ToList();
        return new KeyEventsResponse
        {
            Items = items,
            Pagination = new KeyEventsPagination { Total = total, Limit = limit, Offset = offset, HasMore = offset + limit < total }
        };
    }

    public async Task<TimelineEntryDetail?> GetKeyEventByIdAsync(string eventId, string tenantId, CancellationToken ct = default)
    {
        var evt = await _eventRepo.GetByIdAsync(eventId, tenantId, ct).ConfigureAwait(false);
        if (evt is null) return null;

        IncidentStateInfo? incidentState = null;
        if (evt.IncidentId is not null)
        {
            var inc = await _incidentRepo.GetByIdAsync(evt.IncidentId, tenantId, ct).ConfigureAwait(false);
            if (inc is not null)
                incidentState = new IncidentStateInfo { IncidentId = inc.Id, State = inc.State.ToString(), Severity = inc.Severity.ToString() };
        }

        string? ruleDescription = null;
        if (evt.ClassificationRuleId is not null)
        {
            var rule = await _ruleRepo.GetByIdAsync(evt.ClassificationRuleId, tenantId, ct).ConfigureAwait(false);
            ruleDescription = rule?.Description;
        }

        return new TimelineEntryDetail
        {
            EventId = evt.Id, TenantId = evt.TenantId, EventType = evt.EventType,
            Severity = evt.Severity.ToString(), Classification = evt.Classification.ToString(),
            ClassificationRuleId = evt.ClassificationRuleId, ClassificationRuleDescription = ruleDescription,
            SourceSystem = evt.SourceSystem, ConnectorId = evt.ConnectorId,
            MonitorId = evt.MonitorId, MonitorName = evt.MonitorName, BusinessPlan = evt.BusinessPlan,
            PipelineId = evt.PipelineId, NotificationStatus = evt.NotificationStatus,
            SourceTimestamp = evt.SourceTimestamp, ClassifiedAt = evt.ClassifiedAt,
            RawPayload = evt.RawPayload, IncidentState = incidentState,
        };
    }

    private async Task<int> CountNonInformationalAsync(string tenantId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct)
    {
        var total = 0;
        foreach (var cls in DefaultClassifications)
            total += await _eventRepo.CountByTenantAsync(tenantId, cls, null, null, null, from, to, ct).ConfigureAwait(false);
        return total;
    }

    private static TimelineEntry MapToTimeline(Event e, Dictionary<string, IncidentRecord> incidents)
    {
        IncidentStateInfo? incState = null;
        if (e.IncidentId is not null && incidents.TryGetValue(e.IncidentId, out var inc))
            incState = new IncidentStateInfo { IncidentId = inc.Id, State = inc.State.ToString(), Severity = inc.Severity.ToString() };
        return new TimelineEntry
        {
            EventId = e.Id, EventType = e.EventType, Severity = e.Severity.ToString(),
            Classification = e.Classification.ToString(), SourceSystem = e.SourceSystem,
            MonitorName = e.MonitorName, BusinessPlan = e.BusinessPlan, PipelineId = e.PipelineId,
            Timestamp = e.SourceTimestamp, IncidentState = incState,
        };
    }
}
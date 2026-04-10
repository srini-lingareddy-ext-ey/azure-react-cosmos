using Todo.Api.Application.Transport;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

public sealed class EventService : IEventService
{
    private readonly IEventRepository _eventRepo;

    public EventService(IEventRepository eventRepo) { _eventRepo = eventRepo; }

    public async Task<EventLogResponse> GetEventsAsync(string tenantId, string? classification, string? severity, string? sourceSystem, string? businessPlan, DateTimeOffset? from, DateTimeOffset? to, int limit, int offset, CancellationToken ct)
    {
        var total = await _eventRepo.CountByTenantAsync(tenantId, classification, severity, sourceSystem, businessPlan, from, to, ct).ConfigureAwait(false);
        var items = new List<EventLogEntry>();
        await foreach (var evt in _eventRepo.GetByTenantAsync(tenantId, classification, severity, sourceSystem, businessPlan, from, to, limit, offset, ct).ConfigureAwait(false))
        {
            items.Add(new EventLogEntry
            {
                EventId = evt.Id,
                EventType = evt.EventType,
                Severity = evt.Severity.ToString(),
                Classification = evt.Classification.ToString(),
                SourceSystem = evt.SourceSystem,
                MonitorName = evt.MonitorName,
                BusinessPlan = evt.BusinessPlan,
                Timestamp = evt.SourceTimestamp,
            });
        }
        return new EventLogResponse { Items = items, Pagination = new EventPagination { Total = total, HasMore = offset + limit < total } };
    }

    public async Task<EventDetail?> GetEventByIdAsync(string eventId, string tenantId, CancellationToken ct)
    {
        var evt = await _eventRepo.GetByIdAsync(eventId, tenantId, ct).ConfigureAwait(false);
        if (evt is null) return null;
        return new EventDetail
        {
            EventId = evt.Id,
            TenantId = evt.TenantId,
            EventType = evt.EventType,
            Severity = evt.Severity.ToString(),
            Classification = evt.Classification.ToString(),
            ClassificationRuleId = evt.ClassificationRuleId,
            SourceSystem = evt.SourceSystem,
            ConnectorId = evt.ConnectorId,
            MonitorId = evt.MonitorId,
            MonitorName = evt.MonitorName,
            BusinessPlan = evt.BusinessPlan,
            PipelineId = evt.PipelineId,
            IncidentId = evt.IncidentId,
            NotificationStatus = evt.NotificationStatus,
            SourceTimestamp = evt.SourceTimestamp,
            ClassifiedAt = evt.ClassifiedAt,
            RawPayload = evt.RawPayload,
        };
    }
}

using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

public interface IEventService
{
    Task<EventLogResponse> GetEventsAsync(string tenantId, string? classification, string? severity, string? sourceSystem, string? businessPlan, DateTimeOffset? from, DateTimeOffset? to, int limit, int offset, CancellationToken ct);
    Task<EventDetail?> GetEventByIdAsync(string eventId, string tenantId, CancellationToken ct);
}

using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

/// <summary>WO-84: Key events timeline service.</summary>
public interface IKeyEventsService
{
    Task<KeyEventsResponse> GetKeyEventsAsync(string tenantId, IReadOnlyList<string>? classifications, DateTimeOffset? from, DateTimeOffset? to, int limit, int offset, CancellationToken ct = default);
    Task<TimelineEntryDetail?> GetKeyEventByIdAsync(string eventId, string tenantId, CancellationToken ct = default);
}
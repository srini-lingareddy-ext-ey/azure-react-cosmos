using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(string eventId, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Event> GetByTenantAsync(string tenantId, string? classification, string? severity, string? sourceSystem, string? businessPlan, DateTimeOffset? from, DateTimeOffset? to, int limit = 50, int offset = 0, CancellationToken cancellationToken = default);
    Task<int> CountByTenantAsync(string tenantId, string? classification, string? severity, string? sourceSystem, string? businessPlan, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default);
    Task<Event> CreateAsync(Event entity, CancellationToken cancellationToken = default);
    Task UpdateIncidentLinkAsync(string eventId, string tenantId, string incidentId, CancellationToken cancellationToken = default);
    Task UpdateNotificationStatusAsync(string eventId, string tenantId, string status, CancellationToken cancellationToken = default);
    Task<int> DeleteOlderThanAsync(string tenantId, DateTimeOffset cutoffDate, CancellationToken cancellationToken = default);
}

using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface INotificationChannelRepository
{
    Task<NotificationChannel?> GetByIdAsync(string id, string tenantId, CancellationToken ct = default);
    IAsyncEnumerable<NotificationChannel> GetAllByTenantAsync(string tenantId, CancellationToken ct = default);
    IAsyncEnumerable<NotificationChannel> GetAllEnabledByTenantAsync(string tenantId, CancellationToken ct = default);
    Task<NotificationChannel> CreateAsync(NotificationChannel channel, CancellationToken ct = default);
    Task<NotificationChannel> UpdateAsync(NotificationChannel channel, CancellationToken ct = default);
    Task DeleteAsync(string id, string tenantId, string? etag = null, CancellationToken ct = default);
}
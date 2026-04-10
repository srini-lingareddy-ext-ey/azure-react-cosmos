using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface INotificationDeliveryLogRepository
{
    IAsyncEnumerable<NotificationDeliveryLog> GetByTenantAsync(string tenantId, string? status, DateTimeOffset? from, DateTimeOffset? to, int limit = 50, int offset = 0, CancellationToken ct = default);
    Task<NotificationDeliveryLog> CreateAsync(NotificationDeliveryLog log, CancellationToken ct = default);
}

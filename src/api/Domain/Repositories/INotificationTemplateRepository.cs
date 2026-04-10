using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface INotificationTemplateRepository
{
    Task<NotificationTemplate?> GetByClassificationAsync(string tenantId, string classification, CancellationToken ct = default);
    Task<NotificationTemplate> UpsertAsync(NotificationTemplate template, CancellationToken ct = default);
}

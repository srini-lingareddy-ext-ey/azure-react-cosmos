using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface INotificationRoutingRuleRepository
{
    IAsyncEnumerable<NotificationRoutingRule> GetAllByTenantAsync(string tenantId, CancellationToken ct = default);
    IAsyncEnumerable<NotificationRoutingRule> GetMatchingRulesAsync(string tenantId, string? scopeValue, string? businessPlan, string? monitorId, IEnumerable<string> classifications, IEnumerable<string> severities, CancellationToken ct = default);
    Task<NotificationRoutingRule> CreateAsync(NotificationRoutingRule rule, CancellationToken ct = default);
    Task<NotificationRoutingRule> UpdateAsync(NotificationRoutingRule rule, CancellationToken ct = default);
    Task DeleteAsync(string id, string tenantId, string? etag = null, CancellationToken ct = default);
}

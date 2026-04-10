using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed notification routing rule repository (WO-65). Partition key /tenantId.</summary>
public sealed class NotificationRoutingRuleRepository : INotificationRoutingRuleRepository
{
    private readonly IRepository<NotificationRoutingRule> _repository;
    public NotificationRoutingRuleRepository(IRepository<NotificationRoutingRule> repository) { _repository = repository; }

    public IAsyncEnumerable<NotificationRoutingRule> GetAllByTenantAsync(string tenantId, CancellationToken ct = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, ct);
    }

    public IAsyncEnumerable<NotificationRoutingRule> GetMatchingRulesAsync(string tenantId, string? scopeValue, string? businessPlan, string? monitorId, IEnumerable<string> classifications, IEnumerable<string> severities, CancellationToken ct = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId AND c.isEnabled = true",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return FilterMatchingRules(spec, scopeValue, businessPlan, monitorId, classifications, severities, ct);
    }

    private async IAsyncEnumerable<NotificationRoutingRule> FilterMatchingRules(
        QuerySpec spec, string? scopeValue, string? businessPlan, string? monitorId,
        IEnumerable<string> classifications, IEnumerable<string> severities,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        var classSet = new HashSet<string>(classifications, StringComparer.OrdinalIgnoreCase);
        var sevSet = new HashSet<string>(severities, StringComparer.OrdinalIgnoreCase);
        await foreach (var rule in _repository.QueryAsync(spec, ct).ConfigureAwait(false))
        {
            if (rule.ScopeType == RoutingScopeType.BusinessPlan && !string.Equals(rule.ScopeValue, businessPlan, StringComparison.OrdinalIgnoreCase)) continue;
            if (rule.ScopeType == RoutingScopeType.Monitor && !string.Equals(rule.ScopeValue, monitorId, StringComparison.OrdinalIgnoreCase)) continue;
            if (rule.Classifications.Count > 0 && !rule.Classifications.Any(c => classSet.Contains(c))) continue;
            if (rule.Severities.Count > 0 && !rule.Severities.Any(s => sevSet.Contains(s))) continue;
            yield return rule;
        }
    }

    public Task<NotificationRoutingRule> CreateAsync(NotificationRoutingRule rule, CancellationToken ct = default) =>
        _repository.CreateAsync(rule, ct);

    public Task<NotificationRoutingRule> UpdateAsync(NotificationRoutingRule rule, CancellationToken ct = default) =>
        _repository.UpdateAsync(rule, ct);

    public Task DeleteAsync(string id, string tenantId, string? etag = null, CancellationToken ct = default) =>
        _repository.DeleteAsync(id, tenantId, etag, ct);
}

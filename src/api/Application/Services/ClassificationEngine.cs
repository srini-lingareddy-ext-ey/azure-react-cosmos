using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

public sealed class ClassificationEngine : IClassificationEngine
{
    private readonly IClassificationRuleRepository _ruleRepo;
    private readonly IClassificationAuditRepository _auditRepo;
    private readonly ITenantRepository _tenantRepo;
    private readonly IDistributedCache _cache;
    private readonly ILogger<ClassificationEngine> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public ClassificationEngine(
        IClassificationRuleRepository ruleRepo,
        IClassificationAuditRepository auditRepo,
        ITenantRepository tenantRepo,
        IDistributedCache cache,
        ILogger<ClassificationEngine> logger)
    {
        _ruleRepo = ruleRepo;
        _auditRepo = auditRepo;
        _tenantRepo = tenantRepo;
        _cache = cache;
        _logger = logger;
    }

    public async Task<(EventClassification classification, string matchedRuleId)> ClassifyAsync(Event evt, CancellationToken ct)
    {
        try
        {
            var rules = await GetRulesAsync(evt.TenantId, ct).ConfigureAwait(false);

            foreach (var rule in rules)
            {
                if (rule.Conditions.Count == 0)
                {
                    _logger.LogWarning("Rule {RuleId} has zero conditions, skipping", rule.RuleId);
                    continue;
                }

                if (EvaluateConditions(rule, evt))
                {
                    var outcome = MapOutcome(rule.Outcome);
                    await WriteAuditAsync(evt, rule.RuleId, rule.Outcome.ToString(), ct).ConfigureAwait(false);
                    return (outcome, rule.RuleId);
                }
            }

            var defaultClassification = await GetDefaultClassificationAsync(evt.TenantId, ct).ConfigureAwait(false);
            await WriteAuditAsync(evt, "default", defaultClassification.ToString(), ct).ConfigureAwait(false);
            return (defaultClassification, "default");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Classification failed for event {EventId}, using default", evt.Id);
            var fallback = await GetDefaultClassificationAsync(evt.TenantId, ct).ConfigureAwait(false);
            await WriteAuditAsync(evt, "error", fallback.ToString(), ct).ConfigureAwait(false);
            return (fallback, "error");
        }
    }

    private async Task<List<ClassificationRule>> GetRulesAsync(string tenantId, CancellationToken ct)
    {
        var cacheKey = $"classification-rules:{tenantId}";
        var cached = await _cache.GetStringAsync(cacheKey, ct).ConfigureAwait(false);
        if (cached is not null)
            return JsonSerializer.Deserialize<List<ClassificationRule>>(cached) ?? new();

        var rules = new List<ClassificationRule>();
        await foreach (var rule in _ruleRepo.GetAllByTenantAsync(tenantId, ct).ConfigureAwait(false))
            rules.Add(rule);

        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(rules),
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheDuration }, ct).ConfigureAwait(false);
        return rules;
    }

    private static bool EvaluateConditions(ClassificationRule rule, Event evt)
    {
        foreach (var condition in rule.Conditions)
        {
            var fieldValue = GetFieldValue(evt, condition.Field);
            if (!EvaluateCondition(fieldValue, condition.Operator, condition.Value))
                return false;
        }
        return true;
    }

    private static string GetFieldValue(Event evt, string field) => field.ToLowerInvariant() switch
    {
        "eventtype" => evt.EventType,
        "severity" => evt.Severity.ToString().ToLowerInvariant(),
        "sourcesystem" => evt.SourceSystem,
        "monitorname" => evt.MonitorName,
        "businessplan" => evt.BusinessPlan ?? string.Empty,
        "classification" => evt.Classification.ToString().ToLowerInvariant(),
        _ => string.Empty,
    };

    private static bool EvaluateCondition(string fieldValue, ConditionOperator op, string ruleValue) => op switch
    {
        ConditionOperator.Equals => string.Equals(fieldValue, ruleValue, StringComparison.OrdinalIgnoreCase),
        ConditionOperator.NotEquals => !string.Equals(fieldValue, ruleValue, StringComparison.OrdinalIgnoreCase),
        ConditionOperator.Contains => fieldValue.Contains(ruleValue, StringComparison.OrdinalIgnoreCase),
        ConditionOperator.GreaterThan => string.Compare(fieldValue, ruleValue, StringComparison.OrdinalIgnoreCase) > 0,
        ConditionOperator.LessThan => string.Compare(fieldValue, ruleValue, StringComparison.OrdinalIgnoreCase) < 0,
        _ => false,
    };

    private static EventClassification MapOutcome(ClassificationOutcome outcome) => outcome switch
    {
        ClassificationOutcome.Informational => EventClassification.Informational,
        ClassificationOutcome.Alert => EventClassification.Alert,
        ClassificationOutcome.AvailabilityIssue => EventClassification.AvailabilityIssue,
        ClassificationOutcome.SlaBreach => EventClassification.SlaBreach,
        ClassificationOutcome.Incident => EventClassification.Incident,
        _ => EventClassification.Informational,
    };

    private async Task<EventClassification> GetDefaultClassificationAsync(string tenantId, CancellationToken ct)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct).ConfigureAwait(false);
        return EventClassification.Informational;
    }

    private async Task WriteAuditAsync(Event evt, string matchedRuleId, string outcome, CancellationToken ct)
    {
        var entry = new ClassificationAuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = evt.TenantId,
            EventId = evt.Id,
            MatchedRuleId = matchedRuleId,
            Outcome = outcome,
            ClassifiedAt = DateTimeOffset.UtcNow,
            EventType = evt.EventType,
            SourceSystem = evt.SourceSystem,
            MonitorName = evt.MonitorName,
        };
        await _auditRepo.CreateAsync(entry, ct).ConfigureAwait(false);
    }
}

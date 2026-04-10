namespace Todo.Api.Application.Transport;

public sealed class ClassificationRuleView
{
    public string RuleId { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public DateTimeOffset? DeployedAt { get; set; }
    public List<RuleConditionView> Conditions { get; set; } = new();
}

public sealed class RuleConditionView
{
    public string Field { get; set; } = string.Empty;
    public string Operator { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}

public sealed class ClassificationAuditEntryResponse
{
    public string Id { get; set; } = string.Empty;
    public string EventId { get; set; } = string.Empty;
    public string MatchedRuleId { get; set; } = string.Empty;
    public string Outcome { get; set; } = string.Empty;
    public DateTimeOffset? ClassifiedAt { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string SourceSystem { get; set; } = string.Empty;
    public string MonitorName { get; set; } = string.Empty;
}

public sealed class ReloadRequest
{
    public List<string> TenantIds { get; set; } = new();
}

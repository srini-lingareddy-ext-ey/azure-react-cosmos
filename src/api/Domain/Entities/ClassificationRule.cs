namespace Todo.Api.Domain.Entities;

public enum ConditionOperator { Equals = 0, NotEquals = 1, Contains = 2, GreaterThan = 3, LessThan = 4 }
public enum ClassificationOutcome { Informational = 0, Alert = 1, AvailabilityIssue = 2, SlaBreach = 3, Incident = 4 }

public sealed class RuleCondition
{
    public string Field { get; set; } = string.Empty;
    public ConditionOperator Operator { get; set; }
    public string Value { get; set; } = string.Empty;
}

public sealed class ClassificationRule : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string RuleId { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<RuleCondition> Conditions { get; set; } = new();
    public ClassificationOutcome Outcome { get; set; }
    public DateTimeOffset? DeployedAt { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

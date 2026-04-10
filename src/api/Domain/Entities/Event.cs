namespace Todo.Api.Domain.Entities;

public enum EventSeverity { Info = 0, Warning = 1, Critical = 2 }
public enum EventClassification { Informational = 0, Alert = 1, AvailabilityIssue = 2, SlaBreach = 3, Incident = 4 }

public sealed class Event : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string SourceSystem { get; set; } = string.Empty;
    public string ConnectorId { get; set; } = string.Empty;
    public string MonitorId { get; set; } = string.Empty;
    public string MonitorName { get; set; } = string.Empty;
    public string? BusinessPlan { get; set; }
    public string EventType { get; set; } = string.Empty;
    public EventSeverity Severity { get; set; }
    public EventClassification Classification { get; set; }
    public string? ClassificationRuleId { get; set; }
    public DateTimeOffset? SourceTimestamp { get; set; }
    public DateTimeOffset? IngestionTimestamp { get; set; }
    public DateTimeOffset? NormalizedAt { get; set; }
    public DateTimeOffset? EnrichedAt { get; set; }
    public DateTimeOffset? ClassifiedAt { get; set; }
    public string? PipelineId { get; set; }
    public string? IncidentId { get; set; }
    public string? NotificationStatus { get; set; }
    public Dictionary<string, object> RawPayload { get; set; } = new();
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

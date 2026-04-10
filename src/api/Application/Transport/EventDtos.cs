namespace Todo.Api.Application.Transport;

public sealed class EventLogEntry
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public string SourceSystem { get; set; } = string.Empty;
    public string MonitorName { get; set; } = string.Empty;
    public string? BusinessPlan { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}

public sealed class EventDetail
{
    public string EventId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public string? ClassificationRuleId { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string ConnectorId { get; set; } = string.Empty;
    public string MonitorId { get; set; } = string.Empty;
    public string MonitorName { get; set; } = string.Empty;
    public string? BusinessPlan { get; set; }
    public string? PipelineId { get; set; }
    public string? IncidentId { get; set; }
    public string? NotificationStatus { get; set; }
    public DateTimeOffset? SourceTimestamp { get; set; }
    public DateTimeOffset? ClassifiedAt { get; set; }
    public Dictionary<string, object> RawPayload { get; set; } = new();
}

public sealed class EventLogResponse
{
    public List<EventLogEntry> Items { get; set; } = new();
    public EventPagination Pagination { get; set; } = new();
}

public sealed class EventPagination
{
    public int Total { get; set; }
    public bool HasMore { get; set; }
}

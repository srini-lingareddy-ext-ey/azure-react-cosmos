namespace Todo.Api.Application.Transport;

/// <summary>WO-84: Key events timeline entry.</summary>
public sealed class TimelineEntry
{
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public string SourceSystem { get; set; } = string.Empty;
    public string MonitorName { get; set; } = string.Empty;
    public string? BusinessPlan { get; set; }
    public string? PipelineId { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public IncidentStateInfo? IncidentState { get; set; }
}

public sealed class IncidentStateInfo
{
    public string IncidentId { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
}

/// <summary>WO-84: Key events timeline detail.</summary>
public sealed class TimelineEntryDetail
{
    public string EventId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Classification { get; set; } = string.Empty;
    public string? ClassificationRuleId { get; set; }
    public string? ClassificationRuleDescription { get; set; }
    public string SourceSystem { get; set; } = string.Empty;
    public string ConnectorId { get; set; } = string.Empty;
    public string MonitorId { get; set; } = string.Empty;
    public string MonitorName { get; set; } = string.Empty;
    public string? BusinessPlan { get; set; }
    public string? PipelineId { get; set; }
    public string? NotificationStatus { get; set; }
    public DateTimeOffset? SourceTimestamp { get; set; }
    public DateTimeOffset? ClassifiedAt { get; set; }
    public Dictionary<string, object> RawPayload { get; set; } = new();
    public IncidentStateInfo? IncidentState { get; set; }
}

/// <summary>WO-84: Paginated key events timeline response.</summary>
public sealed class KeyEventsResponse
{
    public IReadOnlyList<TimelineEntry> Items { get; set; } = Array.Empty<TimelineEntry>();
    public KeyEventsPagination Pagination { get; set; } = new();
}

public sealed class KeyEventsPagination
{
    public int Total { get; set; }
    public int Limit { get; set; }
    public int Offset { get; set; }
    public bool HasMore { get; set; }
}
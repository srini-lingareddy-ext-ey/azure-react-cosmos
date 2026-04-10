namespace Todo.Api.Application.Transport;

/// <summary>WO-70: incident list entry.</summary>
public sealed class IncidentListEntry
{
    public string Id { get; set; } = string.Empty;
    public string DisplayId { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string MonitorId { get; set; } = string.Empty;
    public string MonitorName { get; set; } = string.Empty;
    public string BusinessPlan { get; set; } = string.Empty;
    public int RecurrenceCount { get; set; }
    public string TicketCreationStatus { get; set; } = string.Empty;
    public string? ServiceNowTicketNumber { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>WO-70: incident list response with pagination.</summary>
public sealed class IncidentListResponse
{
    public IReadOnlyList<IncidentListEntry> Items { get; set; } = Array.Empty<IncidentListEntry>();
    public PaginationInfo Pagination { get; set; } = new();
}

public sealed class PaginationInfo
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

/// <summary>WO-70: full incident detail DTO.</summary>
public sealed class IncidentDetailDto
{
    public string Id { get; set; } = string.Empty;
    public string DisplayId { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string MonitorId { get; set; } = string.Empty;
    public string MonitorName { get; set; } = string.Empty;
    public string BusinessPlan { get; set; } = string.Empty;
    public string? AffectedPipelineId { get; set; }
    public string TriggeringEventId { get; set; } = string.Empty;
    public int RecurrenceCount { get; set; }
    public string? ResolutionNote { get; set; }
    public ServiceNowPanel ServiceNow { get; set; } = new();
    public List<StateHistoryDto> StateHistory { get; set; } = new();
    public List<IncidentNoteDto> Notes { get; set; } = new();
    public string? Etag { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public sealed class ServiceNowPanel
{
    public string TicketCreationStatus { get; set; } = string.Empty;
    public int TicketCreationRetries { get; set; }
    public string? TicketNumber { get; set; }
    public string? TicketUrl { get; set; }
    public string? TicketStatus { get; set; }
    public DateTimeOffset? LastSyncedAt { get; set; }
}

public sealed class StateHistoryDto
{
    public string? FromState { get; set; }
    public string ToState { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string? Note { get; set; }
}

public sealed class IncidentNoteDto
{
    public string NoteId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public bool SyncedToServiceNow { get; set; }
}

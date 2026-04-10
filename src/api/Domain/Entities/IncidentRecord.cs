namespace Todo.Api.Domain.Entities;

public enum IncidentSeverity { Critical = 0, High = 1, Medium = 2, Low = 3 }
public enum IncidentState { Open = 0, InProgress = 1, Resolved = 2, Closed = 3 }
public enum TicketCreationStatus { Pending = 0, Created = 1, Failed = 2 }

public sealed class StateHistoryEntry
{
    public string? FromState { get; set; }
    public string ToState { get; set; } = string.Empty;
    public string Actor { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string? Note { get; set; }
}

public sealed class IncidentNote
{
    public string NoteId { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public bool SyncedToServiceNow { get; set; }
}

public sealed class IncidentRecord : IDomainEntity, IConcurrencyEntity, IAuditableEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TenantId { get; set; } = string.Empty;
    public string DisplayId { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; } = IncidentSeverity.High;
    public IncidentState State { get; set; } = IncidentState.Open;
    public string MonitorId { get; set; } = string.Empty;
    public string MonitorName { get; set; } = string.Empty;
    public string BusinessPlan { get; set; } = string.Empty;
    public string? AffectedPipelineId { get; set; }
    public string TriggeringEventId { get; set; } = string.Empty;
    public int RecurrenceCount { get; set; }
    public string? ResolutionNote { get; set; }
    public TicketCreationStatus TicketCreationStatus { get; set; } = TicketCreationStatus.Pending;
    public int TicketCreationRetries { get; set; }
    public string? ServiceNowTicketNumber { get; set; }
    public string? ServiceNowTicketUrl { get; set; }
    public string? ServiceNowTicketStatus { get; set; }
    public DateTimeOffset? LastSyncedAt { get; set; }
    public List<StateHistoryEntry> StateHistory { get; set; } = new();
    public List<IncidentNote> Notes { get; set; } = new();
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public object PartitionKeyValue => TenantId;
}
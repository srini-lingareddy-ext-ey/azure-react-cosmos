namespace Todo.Api.Application.Transport;

/// <summary>WO-67: PATCH incidents/{id}/state</summary>
public sealed class StateTransitionRequest
{
    public string ToState { get; set; } = string.Empty;
    public string? ResolutionNote { get; set; }
}

public sealed record StateTransitionResponse(string State, string Etag);

/// <summary>WO-67: POST incidents/{id}/notes</summary>
public sealed class AddNoteRequest
{
    public string Content { get; set; } = string.Empty;
}

public sealed record AddNoteResponse(string NoteId);

/// <summary>WO-67: POST incidents/{id}/retry-ticket</summary>
public sealed record RetryTicketResponse(string? TicketNumber, string TicketCreationStatus);

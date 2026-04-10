using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

/// <summary>WO-67: incident state transitions, notes, and ticket retry.</summary>
public interface IIncidentLifecycleService
{
    Task<StateTransitionResponse> TransitionStateAsync(string incidentId, string tenantId, string userId, StateTransitionRequest request, string? etag = null, CancellationToken cancellationToken = default);
    Task<AddNoteResponse> AddNoteAsync(string incidentId, string tenantId, string userId, string userName, AddNoteRequest request, CancellationToken cancellationToken = default);
    Task<RetryTicketResponse> RetryTicketAsync(string incidentId, string tenantId, CancellationToken cancellationToken = default);
}

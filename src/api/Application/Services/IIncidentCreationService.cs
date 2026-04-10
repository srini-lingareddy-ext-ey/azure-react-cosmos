namespace Todo.Api.Application.Services;

/// <summary>WO-66: Creates incident records from event data with deduplication.</summary>
public interface IIncidentCreationService
{
    Task<string> CreateAsync(
        string tenantId, string monitorId, string monitorName, string businessPlan,
        string? pipelineId, string eventId, string eventSeverity,
        CancellationToken cancellationToken = default);
}

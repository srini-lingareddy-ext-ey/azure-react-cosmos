using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

/// <summary>WO-70: incident list and detail queries.</summary>
public interface IIncidentQueryService
{
    Task<IncidentListResponse> GetIncidentsAsync(string tenantId, string? state, string? severity, DateTimeOffset? from, DateTimeOffset? to, string? sort, string? order, int limit, int offset, CancellationToken cancellationToken = default);
    Task<IncidentDetailDto> GetIncidentByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
}

using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

/// <summary>WO-46: monitor CRUD with pause/activate.</summary>
public interface IMonitorService
{
    Task<MonitorListResponse> ListAsync(string tenantId, string? status, string? businessPlanId, CancellationToken cancellationToken = default);
    Task<MonitorResponse> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    Task<MonitorResponse> CreateAsync(string userId, string tenantId, CreateMonitorRequest request, CancellationToken cancellationToken = default);
    Task<MonitorResponse> UpdateAsync(string userId, string id, string tenantId, UpdateMonitorRequest request, CancellationToken cancellationToken = default);
    Task<MonitorResponse> PauseAsync(string userId, string id, string tenantId, CancellationToken cancellationToken = default);
    Task<MonitorResponse> ActivateAsync(string userId, string id, string tenantId, CancellationToken cancellationToken = default);
}

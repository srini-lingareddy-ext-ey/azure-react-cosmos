using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IMaintenanceWindowRepository
{
    IAsyncEnumerable<MaintenanceWindow> GetActiveWindowsAsync(string tenantId, string? monitorId, string? businessPlan, DateTimeOffset at, CancellationToken ct = default);
    IAsyncEnumerable<MaintenanceWindow> GetAllByTenantAsync(string tenantId, CancellationToken ct = default);
    Task<MaintenanceWindow> CreateAsync(MaintenanceWindow window, CancellationToken ct = default);
    Task DeleteAsync(string id, string tenantId, string? etag = null, CancellationToken ct = default);
}

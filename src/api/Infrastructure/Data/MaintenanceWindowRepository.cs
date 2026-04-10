using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed maintenance window repository (WO-65). Partition key /tenantId.</summary>
public sealed class MaintenanceWindowRepository : IMaintenanceWindowRepository
{
    private readonly IRepository<MaintenanceWindow> _repository;
    public MaintenanceWindowRepository(IRepository<MaintenanceWindow> repository) { _repository = repository; }

    public IAsyncEnumerable<MaintenanceWindow> GetActiveWindowsAsync(string tenantId, string? monitorId, string? businessPlan, DateTimeOffset at, CancellationToken ct = default)
    {
        var spec = new QuerySpec(
            "SELECT * FROM c WHERE c.tenantId = @tenantId AND c.startTime <= @at AND c.endTime >= @at",
            new Dictionary<string, object> { ["@tenantId"] = tenantId, ["@at"] = at });
        return FilterActiveWindows(spec, monitorId, businessPlan, ct);
    }

    private async IAsyncEnumerable<MaintenanceWindow> FilterActiveWindows(
        QuerySpec spec, string? monitorId, string? businessPlan,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        await foreach (var window in _repository.QueryAsync(spec, ct).ConfigureAwait(false))
        {
            if (window.ScopeType == RoutingScopeType.All) { yield return window; continue; }
            if (window.ScopeType == RoutingScopeType.BusinessPlan && string.Equals(window.ScopeValue, businessPlan, StringComparison.OrdinalIgnoreCase)) { yield return window; continue; }
            if (window.ScopeType == RoutingScopeType.Monitor && string.Equals(window.ScopeValue, monitorId, StringComparison.OrdinalIgnoreCase)) { yield return window; continue; }
        }
    }

    public IAsyncEnumerable<MaintenanceWindow> GetAllByTenantAsync(string tenantId, CancellationToken ct = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, ct);
    }

    public Task<MaintenanceWindow> CreateAsync(MaintenanceWindow window, CancellationToken ct = default) =>
        _repository.CreateAsync(window, ct);

    public Task DeleteAsync(string id, string tenantId, string? etag = null, CancellationToken ct = default) =>
        _repository.DeleteAsync(id, tenantId, etag, ct);
}

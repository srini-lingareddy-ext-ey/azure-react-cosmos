using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

/// <summary>WO-83: Dashboard KPIs and Charts API.</summary>
public interface IDashboardChartsService
{
    Task<DashboardKpisResponse> GetKpisAsync(string tenantId, CancellationToken ct = default);
    Task<DashboardChartsResponse> GetChartsAsync(string tenantId, string timeRange, CancellationToken ct = default);
}
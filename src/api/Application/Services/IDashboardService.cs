using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Services;

/// <summary>WO-82: Health Bar and Dashboard Config API.</summary>
public interface IDashboardService
{
    Task<HealthScore?> GetHealthScoreAsync(string tenantId, CancellationToken ct = default);
    Task<DashboardConfigResponse> GetDashboardConfigAsync(string tenantId, string userRole, CancellationToken ct = default);
}

public sealed class DashboardConfigResponse
{
    public string DefaultTab { get; set; } = "dataPipelines";
    public List<string> VisibleModules { get; set; } = new();
    public string TenantName { get; set; } = string.Empty;
    public TenantBranding? Branding { get; set; }
}
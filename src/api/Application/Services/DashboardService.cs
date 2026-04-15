using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>WO-82: Health score read + dashboard config.</summary>
public sealed class DashboardService : IDashboardService
{
    private static readonly List<string> AllModules = new() { "dataPipelines", "dataQuality", "infrastructure", "events", "incidents" };
    private readonly IHealthScoreRepository _healthScoreRepo;
    private readonly ITenantRepository _tenantRepo;

    public DashboardService(IHealthScoreRepository healthScoreRepo, ITenantRepository tenantRepo)
    { _healthScoreRepo = healthScoreRepo; _tenantRepo = tenantRepo; }

    public async Task<HealthScore?> GetHealthScoreAsync(string tenantId, CancellationToken ct = default)
    {
        var hs = await _healthScoreRepo.GetByTenantIdAsync(tenantId, ct).ConfigureAwait(false);
        if (hs is null) return null;

        if (hs.CalculatedAt.HasValue && (DateTimeOffset.UtcNow - hs.CalculatedAt.Value).TotalMinutes > 10)
            hs.IsStale = true;

        return hs;
    }

    public async Task<DashboardConfigResponse> GetDashboardConfigAsync(string tenantId, string userRole, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByIdAsync(tenantId, ct).ConfigureAwait(false);
        return new DashboardConfigResponse
        {
            DefaultTab = "dataPipelines",
            VisibleModules = AllModules,
            TenantName = tenant?.DisplayName ?? tenant?.Name ?? tenantId,
            Branding = tenant?.Branding
        };
    }
}
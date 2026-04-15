using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.BackgroundJobs;

/// <summary>WO-79: 5-minute job - aggregates KPI metrics into DashboardSummary per tenant.</summary>
public sealed class DashboardKpiAggregationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DashboardKpiAggregationJob> _logger;

    public DashboardKpiAggregationJob(IServiceProvider serviceProvider, ILogger<DashboardKpiAggregationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        _logger.LogInformation("DashboardKpiAggregationJob started (5-minute cycle)");

        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await RunCycleAsync(scope.ServiceProvider, stoppingToken).ConfigureAwait(false);
                _logger.LogDebug("Dashboard KPI aggregation cycle completed");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Dashboard KPI aggregation cycle failed");
            }
        }
    }

    private static async Task RunCycleAsync(IServiceProvider sp, CancellationToken ct)
    {
        var tenantRepo = sp.GetRequiredService<ITenantRepository>();
        var incidentRepo = sp.GetRequiredService<IIncidentRepository>();
        var slaStatusRepo = sp.GetRequiredService<IPipelineSLAStatusRepository>();
        var eventRepo = sp.GetRequiredService<IEventRepository>();
        var dqStatusRepo = sp.GetRequiredService<IDataQualityStatusRepository>();
        var infraRepo = sp.GetRequiredService<IComponentHealthStatusRepository>();
        var summaryRepo = sp.GetRequiredService<IDashboardSummaryRepository>();

        var now = DateTimeOffset.UtcNow;

        await foreach (var tenant in tenantRepo.GetAllAsync(ct).ConfigureAwait(false))
        {
            var activeIncidents = 0;
            await foreach (var inc in incidentRepo.GetByTenantAsync(tenant.Id, null, null, null, null, null, null, 10000, 0, ct).ConfigureAwait(false))
            {
                if (inc.State is IncidentState.Open or IncidentState.InProgress)
                    activeIncidents++;
            }

            int slaMet = 0, slaTotal = 0;
            await foreach (var sla in slaStatusRepo.GetAllByTenantAsync(tenant.Id, ct).ConfigureAwait(false))
            {
                slaTotal++;
                if (sla.Status == SLAStatus.Met)
                    slaMet++;
            }
            double slaRate = slaTotal > 0 ? Math.Round((double)slaMet / slaTotal * 100, 1) : 100;

            int criticalAlerts = 0;
            var cutoff24h = now.AddHours(-24);
            await foreach (var evt in eventRepo.GetByTenantAsync(tenant.Id, "Alert", "Critical", null, null, cutoff24h, null, 10000, 0, ct).ConfigureAwait(false))
            {
                criticalAlerts++;
            }

            int dqPassing = 0, dqTotal = 0;
            await foreach (var dq in dqStatusRepo.GetAllByTenantAsync(tenant.Id, ct).ConfigureAwait(false))
            {
                dqTotal++;
                if (dq.QualityStatusValue == QualityStatus.Passing)
                    dqPassing++;
            }
            double dqRate = dqTotal > 0 ? Math.Round((double)dqPassing / dqTotal * 100, 1) : 100;

            int degraded = 0;
            await foreach (var comp in infraRepo.GetAllByTenantAsync(tenant.Id, ct).ConfigureAwait(false))
            {
                if (comp.Status is InfraHealthState.Warning or InfraHealthState.Critical)
                    degraded++;
            }

            var existing = await summaryRepo.GetByTenantIdAsync(tenant.Id, ct).ConfigureAwait(false)
                ?? new DashboardSummary { Id = tenant.Id, TenantId = tenant.Id };

            existing.ActiveIncidentsPrior = existing.ActiveIncidents;
            existing.SlaComplianceRatePrior = existing.SlaComplianceRate;
            existing.OpenCriticalAlertsPrior = existing.OpenCriticalAlerts;
            existing.DataQualityPassRatePrior = existing.DataQualityPassRate;
            existing.DegradedInfraComponentsPrior = existing.DegradedInfraComponents;

            existing.ActiveIncidents = activeIncidents;
            existing.SlaComplianceRate = slaRate;
            existing.OpenCriticalAlerts = criticalAlerts;
            existing.DataQualityPassRate = dqRate;
            existing.DegradedInfraComponents = degraded;
            existing.CalculatedAt = now;

            await summaryRepo.UpsertAsync(existing, ct).ConfigureAwait(false);
        }
    }
}
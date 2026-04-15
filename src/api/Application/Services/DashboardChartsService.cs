using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>WO-83: reads DashboardSummary for KPIs, queries source containers for charts.</summary>
public sealed class DashboardChartsService : IDashboardChartsService
{
    private readonly IDashboardSummaryRepository _summaryRepo;
    private readonly IPipelineExecutionRepository _execRepo;
    private readonly IIncidentRepository _incidentRepo;
    private readonly IDimensionSnapshotRepository _snapshotRepo;

    public DashboardChartsService(
        IDashboardSummaryRepository summaryRepo,
        IPipelineExecutionRepository execRepo,
        IIncidentRepository incidentRepo,
        IDimensionSnapshotRepository snapshotRepo)
    {
        _summaryRepo = summaryRepo;
        _execRepo = execRepo;
        _incidentRepo = incidentRepo;
        _snapshotRepo = snapshotRepo;
    }

    public async Task<DashboardKpisResponse> GetKpisAsync(string tenantId, CancellationToken ct = default)
    {
        var summary = await _summaryRepo.GetByTenantIdAsync(tenantId, ct).ConfigureAwait(false);
        if (summary is null)
        {
            return new DashboardKpisResponse
            {
                ActiveIncidents = new KpiValue { Value = null, TrendDirection = null },
                SlaComplianceRate = new KpiValue { Value = null, Unit = "%", TrendDirection = null },
                OpenCriticalAlerts = new KpiValue { Value = null, TrendDirection = null },
                DataQualityPassRate = new KpiValue { Value = null, Unit = "%", TrendDirection = null },
                DegradedInfraComponents = new KpiValue { Value = null, TrendDirection = null }
            };
        }

        return new DashboardKpisResponse
        {
            ActiveIncidents = BuildKpi(summary.ActiveIncidents, summary.ActiveIncidentsPrior),
            SlaComplianceRate = BuildKpi(summary.SlaComplianceRate, summary.SlaComplianceRatePrior, "%"),
            OpenCriticalAlerts = BuildKpi(summary.OpenCriticalAlerts, summary.OpenCriticalAlertsPrior),
            DataQualityPassRate = BuildKpi(summary.DataQualityPassRate, summary.DataQualityPassRatePrior, "%"),
            DegradedInfraComponents = BuildKpi(summary.DegradedInfraComponents, summary.DegradedInfraComponentsPrior)
        };
    }

    public async Task<DashboardChartsResponse> GetChartsAsync(string tenantId, string timeRange, CancellationToken ct = default)
    {
        var (from, bucketCount, bucketFormat) = ParseTimeRange(timeRange);
        var to = DateTimeOffset.UtcNow;

        var pipelineOutcomes = new ChartSeries { Name = "pipelineOutcomes" };
        var incidentVolume = new ChartSeries { Name = "incidentVolume" };
        var dqTrend = new ChartSeries { Name = "dataQualityTrend" };
        var infraTrend = new ChartSeries { Name = "infraHealthTrend" };

        for (int i = 0; i < bucketCount; i++)
        {
            var label = from.AddDays(i).ToString(bucketFormat);
            pipelineOutcomes.DataPoints.Add(new ChartDataPoint { Date = label, Value = 0, Value2 = 0 });
            incidentVolume.DataPoints.Add(new ChartDataPoint { Date = label, Value = 0 });
            dqTrend.DataPoints.Add(new ChartDataPoint { Date = label, Value = null });
            infraTrend.DataPoints.Add(new ChartDataPoint { Date = label, Value = null });
        }

        await foreach (var snap in _snapshotRepo.GetAllByTenantAsync(tenantId, ct).ConfigureAwait(false))
        {
            if (snap.CapturedAt is null || snap.CapturedAt < from || snap.CapturedAt > to) continue;
            int idx = (int)(snap.CapturedAt.Value - from).TotalDays;
            if (idx < 0 || idx >= bucketCount) continue;

            if (snap.DimensionKey == "dataQuality")
                dqTrend.DataPoints[idx] = new ChartDataPoint { Date = dqTrend.DataPoints[idx].Date, Value = snap.Score };
            else if (snap.DimensionKey == "infrastructure")
                infraTrend.DataPoints[idx] = new ChartDataPoint { Date = infraTrend.DataPoints[idx].Date, Value = snap.Score };
        }

        return new DashboardChartsResponse
        {
            PipelineOutcomes = pipelineOutcomes,
            IncidentVolume = incidentVolume,
            DataQualityTrend = dqTrend,
            InfraHealthTrend = infraTrend
        };
    }

    private static KpiValue BuildKpi(double current, double prior, string? unit = null)
    {
        string direction = current > prior ? "up" : current < prior ? "down" : "neutral";
        return new KpiValue { Value = current, Unit = unit, TrendDirection = direction };
    }

    private static (DateTimeOffset From, int BucketCount, string BucketFormat) ParseTimeRange(string timeRange) => timeRange switch
    {
        "last24h" => (DateTimeOffset.UtcNow.AddHours(-24), 24, "HH:00"),
        "last30d" => (DateTimeOffset.UtcNow.AddDays(-30).Date, 30, "yyyy-MM-dd"),
        _ => (DateTimeOffset.UtcNow.AddDays(-7).Date, 7, "yyyy-MM-dd")
    };
}
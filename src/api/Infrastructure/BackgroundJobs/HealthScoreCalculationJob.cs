using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.BackgroundJobs;

/// <summary>WO-78: 5-minute job - reads DimensionSnapshots, computes weighted HealthScore per tenant.</summary>
public sealed class HealthScoreCalculationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HealthScoreCalculationJob> _logger;

    public HealthScoreCalculationJob(IServiceProvider serviceProvider, ILogger<HealthScoreCalculationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        _logger.LogInformation("HealthScoreCalculationJob started (5-minute cycle)");

        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await RunCycleAsync(scope.ServiceProvider, stoppingToken).ConfigureAwait(false);
                _logger.LogDebug("Health score calculation cycle completed");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Health score calculation cycle failed");
            }
        }
    }

    private static async Task RunCycleAsync(IServiceProvider sp, CancellationToken ct)
    {
        var tenantRepo = sp.GetRequiredService<ITenantRepository>();
        var snapshotRepo = sp.GetRequiredService<IDimensionSnapshotRepository>();
        var healthScoreRepo = sp.GetRequiredService<IHealthScoreRepository>();

        await foreach (var tenant in tenantRepo.GetAllAsync(ct).ConfigureAwait(false))
        {
            var weights = tenant.Config?.HealthScoreWeights ?? new Dictionary<string, double>();
            if (weights.Count == 0) continue;

            var snapshots = new Dictionary<string, DimensionSnapshot>();
            await foreach (var s in snapshotRepo.GetAllByTenantAsync(tenant.Id, ct).ConfigureAwait(false))
            {
                if (!snapshots.ContainsKey(s.DimensionKey) ||
                    (s.CapturedAt ?? DateTimeOffset.MinValue) > (snapshots[s.DimensionKey].CapturedAt ?? DateTimeOffset.MinValue))
                {
                    snapshots[s.DimensionKey] = s;
                }
            }

            var thresholds = tenant.Config?.HealthStatusThresholds;
            double healthyMin = thresholds?.HealthyMin ?? 85;
            double warningMin = thresholds?.WarningMin ?? 60;

            double weightedSum = 0;
            double totalWeight = 0;
            bool isStale = false;
            var dimensions = new List<HealthDimension>();
            var now = DateTimeOffset.UtcNow;

            foreach (var (key, weight) in weights)
            {
                if (!snapshots.TryGetValue(key, out var snap))
                {
                    dimensions.Add(new HealthDimension { Key = key, Label = FormatLabel(key), Score = 0, Status = HealthScoreStatus.Red, Weight = weight, IsActive = false });
                    continue;
                }

                if (snap.CapturedAt.HasValue && (now - snap.CapturedAt.Value).TotalMinutes > 10)
                    isStale = true;

                var dimStatus = snap.Score >= healthyMin ? HealthScoreStatus.Green
                    : snap.Score >= warningMin ? HealthScoreStatus.Yellow
                    : HealthScoreStatus.Red;

                dimensions.Add(new HealthDimension { Key = key, Label = FormatLabel(key), Score = snap.Score, Status = dimStatus, Weight = weight, IsActive = true });
                weightedSum += snap.Score * weight;
                totalWeight += weight;
            }

            double compositeScore = totalWeight > 0 ? Math.Round(weightedSum / totalWeight, 1) : 0;
            var status = compositeScore >= healthyMin ? HealthScoreStatus.Green
                : compositeScore >= warningMin ? HealthScoreStatus.Yellow
                : HealthScoreStatus.Red;

            var healthScore = await healthScoreRepo.GetByTenantIdAsync(tenant.Id, ct).ConfigureAwait(false)
                ?? new HealthScore { Id = tenant.Id, TenantId = tenant.Id };

            healthScore.Score = compositeScore;
            healthScore.Status = status;
            healthScore.IsStale = isStale;
            healthScore.CalculatedAt = now;
            healthScore.Dimensions = dimensions;
            await healthScoreRepo.UpsertAsync(healthScore, ct).ConfigureAwait(false);
        }
    }

    private static string FormatLabel(string key) => key switch
    {
        "sla" => "SLA Compliance",
        "dataQuality" => "Data Quality",
        "infrastructure" => "Infrastructure",
        "dataPipelines" => "Data Pipelines",
        _ => key
    };
}
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.BackgroundJobs;

public sealed class DataQualityEvaluationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DataQualityEvaluationJob> _logger;

    public DataQualityEvaluationJob(IServiceProvider serviceProvider, ILogger<DataQualityEvaluationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        _logger.LogInformation("DataQualityEvaluationJob started (5-minute cycle)");

        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<Todo.Api.Application.Services.IDataQualityEvaluationService>();
                await service.EvaluateAllAsync(stoppingToken).ConfigureAwait(false);

                await EmitDimensionSnapshotsAsync(scope.ServiceProvider, stoppingToken).ConfigureAwait(false);

                _logger.LogDebug("Data quality evaluation cycle completed");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Data quality evaluation cycle failed");
            }
        }
    }

    private static async Task EmitDimensionSnapshotsAsync(IServiceProvider sp, CancellationToken ct)
    {
        var tenantRepo = sp.GetRequiredService<ITenantRepository>();
        var dqStatusRepo = sp.GetRequiredService<IDataQualityStatusRepository>();
        var snapshotRepo = sp.GetRequiredService<IDimensionSnapshotRepository>();

        await foreach (var tenant in tenantRepo.GetAllAsync(ct).ConfigureAwait(false))
        {
            int passing = 0, total = 0;
            await foreach (var dq in dqStatusRepo.GetAllByTenantAsync(tenant.Id, ct).ConfigureAwait(false))
            {
                total++;
                if (dq.QualityStatusValue == QualityStatus.Passing)
                    passing++;
            }

            double score = total > 0 ? Math.Round((double)passing / total * 100, 1) : 100;
            await snapshotRepo.CreateAsync(new DimensionSnapshot
            {
                TenantId = tenant.Id,
                DimensionKey = "dataQuality",
                Score = score,
                CapturedAt = DateTimeOffset.UtcNow
            }, ct).ConfigureAwait(false);
        }
    }
}
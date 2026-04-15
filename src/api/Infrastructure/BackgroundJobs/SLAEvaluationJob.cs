using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.BackgroundJobs;

public sealed class SLAEvaluationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SLAEvaluationJob> _logger;

    public SLAEvaluationJob(IServiceProvider serviceProvider, ILogger<SLAEvaluationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        _logger.LogInformation("SLAEvaluationJob started (5-minute cycle)");

        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<Todo.Api.Application.Services.ISLAEvaluationService>();
                await service.EvaluateAllAsync(stoppingToken).ConfigureAwait(false);

                await EmitDimensionSnapshotsAsync(scope.ServiceProvider, stoppingToken).ConfigureAwait(false);

                _logger.LogDebug("SLA evaluation cycle completed");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "SLA evaluation cycle failed");
            }
        }
    }

    private static async Task EmitDimensionSnapshotsAsync(IServiceProvider sp, CancellationToken ct)
    {
        var tenantRepo = sp.GetRequiredService<ITenantRepository>();
        var slaStatusRepo = sp.GetRequiredService<IPipelineSLAStatusRepository>();
        var snapshotRepo = sp.GetRequiredService<IDimensionSnapshotRepository>();

        await foreach (var tenant in tenantRepo.GetAllAsync(ct).ConfigureAwait(false))
        {
            int met = 0, total = 0;
            await foreach (var sla in slaStatusRepo.GetAllByTenantAsync(tenant.Id, ct).ConfigureAwait(false))
            {
                total++;
                if (sla.Status == SLAStatus.Met)
                    met++;
            }

            double score = total > 0 ? Math.Round((double)met / total * 100, 1) : 100;
            await snapshotRepo.CreateAsync(new DimensionSnapshot
            {
                TenantId = tenant.Id,
                DimensionKey = "sla",
                Score = score,
                CapturedAt = DateTimeOffset.UtcNow
            }, ct).ConfigureAwait(false);
        }
    }
}
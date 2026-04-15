using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.BackgroundJobs;

public sealed class InfraHealthEvaluationJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<InfraHealthEvaluationJob> _logger;

    public InfraHealthEvaluationJob(IServiceProvider serviceProvider, ILogger<InfraHealthEvaluationJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        _logger.LogInformation("InfraHealthEvaluationJob started (5-minute cycle)");

        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<Todo.Api.Application.Services.IInfraHealthEvaluationService>();
                await service.EvaluateAllAsync(stoppingToken).ConfigureAwait(false);

                await EmitDimensionSnapshotsAsync(scope.ServiceProvider, stoppingToken).ConfigureAwait(false);

                _logger.LogDebug("Infrastructure health evaluation cycle completed");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Infrastructure health evaluation cycle failed");
            }
        }
    }

    private static async Task EmitDimensionSnapshotsAsync(IServiceProvider sp, CancellationToken ct)
    {
        var tenantRepo = sp.GetRequiredService<ITenantRepository>();
        var infraRepo = sp.GetRequiredService<IComponentHealthStatusRepository>();
        var snapshotRepo = sp.GetRequiredService<IDimensionSnapshotRepository>();

        await foreach (var tenant in tenantRepo.GetAllAsync(ct).ConfigureAwait(false))
        {
            int healthy = 0, total = 0;
            await foreach (var comp in infraRepo.GetAllByTenantAsync(tenant.Id, ct).ConfigureAwait(false))
            {
                total++;
                if (comp.Status != InfraHealthState.Warning && comp.Status != InfraHealthState.Critical)
                    healthy++;
            }

            double score = total > 0 ? Math.Round((double)healthy / total * 100, 1) : 100;
            await snapshotRepo.CreateAsync(new DimensionSnapshot
            {
                TenantId = tenant.Id,
                DimensionKey = "infrastructure",
                Score = score,
                CapturedAt = DateTimeOffset.UtcNow
            }, ct).ConfigureAwait(false);
        }
    }
}
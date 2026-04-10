using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.BackgroundJobs;

public sealed class LongRunThresholdJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LongRunThresholdJob> _logger;

    public LongRunThresholdJob(IServiceProvider serviceProvider, ILogger<LongRunThresholdJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LongRunThresholdJob started (nightly)");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(2);
            var delay = nextRun - now;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken).ConfigureAwait(false);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var tenantRepo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
                var jobRunRepo = scope.ServiceProvider.GetRequiredService<IJobRunRepository>();
                var thresholdRepo = scope.ServiceProvider.GetRequiredService<IJobLongRunThresholdRepository>();
                var pipelineRepo = scope.ServiceProvider.GetRequiredService<IPipelineRegistrationRepository>();

                await foreach (var tenant in tenantRepo.GetAllAsync(stoppingToken).ConfigureAwait(false))
                {
                    await foreach (var pipeline in pipelineRepo.GetAllByTenantAsync(tenant.Id, stoppingToken).ConfigureAwait(false))
                    {
                        var jobNames = new HashSet<string>();
                        await foreach (var run in jobRunRepo.GetByJobNameAndPipelineAsync(pipeline.Id, "", tenant.Id, 30, stoppingToken).ConfigureAwait(false))
                        {
                            jobNames.Add(run.JobName);
                        }

                        foreach (var jobName in jobNames)
                        {
                            var runs = new List<JobRun>();
                            await foreach (var r in jobRunRepo.GetByJobNameAndPipelineAsync(pipeline.Id, jobName, tenant.Id, 30, stoppingToken).ConfigureAwait(false))
                            {
                                if (string.Equals(r.Status, "successful", StringComparison.OrdinalIgnoreCase) && r.DurationSeconds.HasValue)
                                    runs.Add(r);
                            }

                            var threshold = new JobLongRunThreshold
                            {
                                TenantId = tenant.Id,
                                PipelineId = pipeline.Id,
                                JobName = jobName,
                                CalculatedAt = DateTimeOffset.UtcNow,
                                CalculatedFromRuns = runs.Count,
                            };

                            if (runs.Count >= 5)
                            {
                                var avg = runs.Average(r => r.DurationSeconds!.Value);
                                threshold.AverageDurationSeconds = avg;
                                threshold.ThresholdSeconds = avg * 2;
                                threshold.IsApplicable = true;
                            }
                            else
                            {
                                threshold.IsApplicable = false;
                                threshold.ThresholdSeconds = null;
                            }

                            await thresholdRepo.UpsertAsync(threshold, stoppingToken).ConfigureAwait(false);
                        }
                    }
                }

                _logger.LogInformation("Nightly long-run threshold recalculation completed");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Nightly long-run threshold job failed");
            }
        }
    }
}

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
                _logger.LogDebug("SLA evaluation cycle completed");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "SLA evaluation cycle failed");
            }
        }
    }
}

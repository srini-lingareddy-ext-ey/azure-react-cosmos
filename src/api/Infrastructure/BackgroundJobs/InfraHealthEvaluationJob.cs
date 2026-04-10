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
                _logger.LogDebug("Infrastructure health evaluation cycle completed");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Infrastructure health evaluation cycle failed");
            }
        }
    }
}

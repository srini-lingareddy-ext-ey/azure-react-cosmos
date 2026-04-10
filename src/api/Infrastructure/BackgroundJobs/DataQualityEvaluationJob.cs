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
                _logger.LogDebug("Data quality evaluation cycle completed");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Data quality evaluation cycle failed");
            }
        }
    }
}

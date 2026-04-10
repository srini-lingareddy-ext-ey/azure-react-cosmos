using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;

namespace Todo.Api.Infrastructure.EventProcessing;

public sealed class EventProcessorBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly KafkaSettings _settings;
    private readonly ILogger<EventProcessorBackgroundService> _logger;

    public EventProcessorBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<KafkaSettings> settings,
        ILogger<EventProcessorBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.BootstrapServers))
        {
            _logger.LogInformation("Kafka not configured — EventProcessor disabled");
            return;
        }

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(_settings.Topics);

        _logger.LogInformation("EventProcessor started — topics: {Topics}", string.Join(", ", _settings.Topics));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(TimeSpan.FromSeconds(1));
                if (result is null) continue;

                await ProcessMessageAsync(result.Message.Value, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EventProcessor unhandled error");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken).ConfigureAwait(false);
            }
        }

        consumer.Close();
    }

    private async Task ProcessMessageAsync(string message, CancellationToken ct)
    {
        string? eventType = null;
        try
        {
            using var doc = JsonDocument.Parse(message);
            eventType = doc.RootElement.TryGetProperty("eventType", out var et) ? et.GetString() : null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Malformed JSON in Kafka message, skipping");
            return;
        }

        if (string.IsNullOrEmpty(eventType))
        {
            _logger.LogWarning("Kafka message missing eventType, skipping");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler>();
        var handler = handlers.FirstOrDefault(h => h.EventType == eventType);

        if (handler is null)
        {
            _logger.LogWarning("Unknown event type: {EventType}, skipping", eventType);
            return;
        }

        await handler.HandleAsync(message, ct).ConfigureAwait(false);
    }
}

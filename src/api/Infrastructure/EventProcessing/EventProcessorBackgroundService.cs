using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Todo.Api.Infrastructure.Configuration;

namespace Todo.Api.Infrastructure.EventProcessing;

public sealed class EventProcessorBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly EventHubSettings _settings;
    private readonly ILogger<EventProcessorBackgroundService> _logger;

    public EventProcessorBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<EventHubSettings> settings,
        ILogger<EventProcessorBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.FullyQualifiedNamespace))
        {
            _logger.LogInformation("EventHubs not configured — EventProcessor disabled");
            return;
        }

        var config = new ConsumerConfig
        {
            BootstrapServers = $"{_settings.FullyQualifiedNamespace}:9093",
            GroupId = _settings.ConsumerGroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = "$ConnectionString",
            SaslPassword = Environment.GetEnvironmentVariable("EVENTHUBS_CONNECTION_STRING") ?? string.Empty,
        };

        var topics = _settings.Hubs.Values.ToArray();
        if (topics.Length == 0)
        {
            _logger.LogWarning("No Event Hub topics configured — EventProcessor disabled");
            return;
        }

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topics);

        _logger.LogInformation("EventProcessor started — topics: {Topics}, group: {GroupId}",
            string.Join(", ", topics), _settings.ConsumerGroupId);

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
            _logger.LogError(ex, "Malformed JSON in Event Hubs message, skipping");
            return;
        }

        if (string.IsNullOrEmpty(eventType))
        {
            _logger.LogWarning("Event Hubs message missing eventType, skipping");
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var handlers = scope.ServiceProvider.GetServices<IEventHandler>();
        var handlerMap = handlers.ToDictionary(h => h.EventType, h => h, StringComparer.OrdinalIgnoreCase);

        if (!handlerMap.TryGetValue(eventType, out var handler))
        {
            _logger.LogWarning("Unknown event type: {EventType}, skipping", eventType);
            return;
        }

        await handler.HandleAsync(message, ct).ConfigureAwait(false);
    }
}

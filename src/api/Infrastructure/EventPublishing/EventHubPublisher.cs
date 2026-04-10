using System.Text;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Todo.Api.Application.EventPublishing;
using Todo.Api.Infrastructure.Configuration;

namespace Todo.Api.Infrastructure.EventPublishing;

/// <summary>
/// WO-48: Confluent.Kafka producer that publishes to Azure Event Hubs via Kafka surface.
/// Singleton — the Kafka producer is long-lived and thread-safe.
/// </summary>
public sealed class EventHubPublisher : IEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly EventHubSettings _settings;
    private readonly ILogger<EventHubPublisher> _logger;

    public EventHubPublisher(IOptions<EventHubSettings> settings, ILogger<EventHubPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        var config = new ProducerConfig
        {
            BootstrapServers = $"{_settings.FullyQualifiedNamespace}:9093",
            SecurityProtocol = SecurityProtocol.SaslSsl,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = "$ConnectionString",
            SaslPassword = Environment.GetEnvironmentVariable("EVENTHUBS_CONNECTION_STRING") ?? string.Empty,
            Acks = Acks.All,
            MessageTimeoutMs = 30000,
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync(string hubName, NormalizedEvent evt, CancellationToken cancellationToken = default)
    {
        var topicName = _settings.Hubs.TryGetValue(hubName, out var resolved) ? resolved : hubName;

        var message = new Message<string, string>
        {
            Key = evt.TenantId,
            Value = evt.Payload,
            Headers = new Headers
            {
                { "eventType", Encoding.UTF8.GetBytes(evt.EventType) },
                { "connectorId", Encoding.UTF8.GetBytes(evt.ConnectorId) },
                { "tenantId", Encoding.UTF8.GetBytes(evt.TenantId) },
                { "timestamp", Encoding.UTF8.GetBytes(evt.Timestamp.ToString("o")) },
            }
        };

        var result = await _producer.ProduceAsync(topicName, message, cancellationToken).ConfigureAwait(false);
        _logger.LogDebug("Published to {Topic} partition {Partition} offset {Offset}", topicName, result.Partition.Value, result.Offset.Value);
    }

    public void Dispose() => _producer.Dispose();
}

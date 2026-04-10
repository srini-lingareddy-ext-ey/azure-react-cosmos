namespace Todo.Api.Infrastructure.EventProcessing;

public sealed class KafkaSettings
{
    public const string SectionName = "Kafka";
    public string BootstrapServers { get; set; } = string.Empty;
    public string ConsumerGroupId { get; set; } = "fdi-monit-consumer";
    public string[] Topics { get; set; } = Array.Empty<string>();
}

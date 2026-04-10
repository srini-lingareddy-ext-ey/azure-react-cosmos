namespace Todo.Api.Infrastructure.Configuration;

public sealed class EventHubSettings
{
    public const string SectionName = "EventHubs";
    public string FullyQualifiedNamespace { get; set; } = string.Empty;
    public string ConsumerGroupId { get; set; } = "a5-event-processor";
    public Dictionary<string, string> Hubs { get; set; } = new();
}

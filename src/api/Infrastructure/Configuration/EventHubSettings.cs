namespace Todo.Api.Infrastructure.Configuration;

/// <summary>
/// WO-41: Strongly-typed options for Azure Event Hubs.
/// Bound to the "EventHubs" configuration section.
/// </summary>
public sealed class EventHubSettings
{
    public const string SectionName = "EventHubs";

    /// <summary>Fully qualified namespace, e.g. "{namespace}.servicebus.windows.net".</summary>
    public string FullyQualifiedNamespace { get; set; } = string.Empty;

    /// <summary>
    /// Maps logical stream names to Event Hub names.
    /// Keys: PipelineEvents, JobEvents, QualityEvents, InfrastructureEvents.
    /// </summary>
    public Dictionary<string, string> Hubs { get; set; } = new();
}

namespace Todo.Api.Infrastructure.EventProcessing;

public interface IEventHandler
{
    string EventType { get; }
    Task HandleAsync(string payload, CancellationToken cancellationToken);
}

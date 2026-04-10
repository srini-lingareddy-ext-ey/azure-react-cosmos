using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.EventProcessing;

public sealed class EventPersistenceService
{
    private readonly IEventRepository _eventRepo;
    private readonly ILogger<EventPersistenceService> _logger;

    public EventPersistenceService(IEventRepository eventRepo, ILogger<EventPersistenceService> logger)
    {
        _eventRepo = eventRepo;
        _logger = logger;
    }

    public async Task PersistAsync(Event evt, CancellationToken ct)
    {
        try
        {
            await _eventRepo.CreateAsync(evt, ct).ConfigureAwait(false);
        }
        catch (Microsoft.Azure.Cosmos.CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            _logger.LogWarning("Duplicate event {EventId} detected, skipping persistence", evt.Id);
        }
    }
}

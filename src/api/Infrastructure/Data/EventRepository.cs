using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class EventRepository : IEventRepository
{
    private readonly IRepository<Event> _repository;
    public EventRepository(IRepository<Event> repository) { _repository = repository; }

    public Task<Event?> GetByIdAsync(string eventId, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(eventId, tenantId, cancellationToken);

    public IAsyncEnumerable<Event> GetByTenantAsync(string tenantId, string? classification, string? severity, string? sourceSystem, string? businessPlan, DateTimeOffset? from, DateTimeOffset? to, int limit = 50, int offset = 0, CancellationToken cancellationToken = default)
    {
        var conditions = new List<string> { "c.tenantId = @tenantId" };
        var parameters = new Dictionary<string, object> { ["@tenantId"] = tenantId };
        if (!string.IsNullOrEmpty(classification)) { conditions.Add("c.classification = @classification"); parameters["@classification"] = Enum.Parse<EventClassification>(classification, true); }
        if (!string.IsNullOrEmpty(severity)) { conditions.Add("c.severity = @severity"); parameters["@severity"] = Enum.Parse<EventSeverity>(severity, true); }
        if (!string.IsNullOrEmpty(sourceSystem)) { conditions.Add("c.sourceSystem = @sourceSystem"); parameters["@sourceSystem"] = sourceSystem; }
        if (!string.IsNullOrEmpty(businessPlan)) { conditions.Add("c.businessPlan = @businessPlan"); parameters["@businessPlan"] = businessPlan; }
        if (from.HasValue) { conditions.Add("c.sourceTimestamp >= @from"); parameters["@from"] = from.Value; }
        if (to.HasValue) { conditions.Add("c.sourceTimestamp <= @to"); parameters["@to"] = to.Value; }
        var where = string.Join(" AND ", conditions);
        var sql = $"SELECT * FROM c WHERE {where} ORDER BY c.sourceTimestamp DESC OFFSET @offset LIMIT @limit";
        parameters["@offset"] = offset;
        parameters["@limit"] = limit;
        return _repository.QueryAsync(new QuerySpec(sql, parameters), cancellationToken);
    }

    public async Task<int> CountByTenantAsync(string tenantId, string? classification, string? severity, string? sourceSystem, string? businessPlan, DateTimeOffset? from, DateTimeOffset? to, CancellationToken cancellationToken = default)
    {
        var conditions = new List<string> { "c.tenantId = @tenantId" };
        var parameters = new Dictionary<string, object> { ["@tenantId"] = tenantId };
        if (!string.IsNullOrEmpty(classification)) { conditions.Add("c.classification = @classification"); parameters["@classification"] = Enum.Parse<EventClassification>(classification, true); }
        if (!string.IsNullOrEmpty(severity)) { conditions.Add("c.severity = @severity"); parameters["@severity"] = Enum.Parse<EventSeverity>(severity, true); }
        if (!string.IsNullOrEmpty(sourceSystem)) { conditions.Add("c.sourceSystem = @sourceSystem"); parameters["@sourceSystem"] = sourceSystem; }
        if (!string.IsNullOrEmpty(businessPlan)) { conditions.Add("c.businessPlan = @businessPlan"); parameters["@businessPlan"] = businessPlan; }
        if (from.HasValue) { conditions.Add("c.sourceTimestamp >= @from"); parameters["@from"] = from.Value; }
        if (to.HasValue) { conditions.Add("c.sourceTimestamp <= @to"); parameters["@to"] = to.Value; }
        var where = string.Join(" AND ", conditions);
        var count = 0;
        await foreach (var _ in _repository.QueryAsync(new QuerySpec($"SELECT c.id FROM c WHERE {where}", parameters), cancellationToken).ConfigureAwait(false))
            count++;
        return count;
    }

    public Task<Event> CreateAsync(Event entity, CancellationToken cancellationToken = default) =>
        _repository.CreateAsync(entity, cancellationToken);

    public async Task UpdateIncidentLinkAsync(string eventId, string tenantId, string incidentId, CancellationToken cancellationToken = default)
    {
        var evt = await _repository.GetByIdAsync(eventId, tenantId, cancellationToken).ConfigureAwait(false);
        if (evt is null) return;
        evt.IncidentId = incidentId;
        await _repository.UpsertAsync(evt, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateNotificationStatusAsync(string eventId, string tenantId, string status, CancellationToken cancellationToken = default)
    {
        var evt = await _repository.GetByIdAsync(eventId, tenantId, cancellationToken).ConfigureAwait(false);
        if (evt is null) return;
        evt.NotificationStatus = status;
        await _repository.UpsertAsync(evt, cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> DeleteOlderThanAsync(string tenantId, DateTimeOffset cutoffDate, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId AND c.sourceTimestamp < @cutoff",
            new Dictionary<string, object> { ["@tenantId"] = tenantId, ["@cutoff"] = cutoffDate });
        var toDelete = new List<Event>();
        await foreach (var evt in _repository.QueryAsync(spec, cancellationToken).ConfigureAwait(false))
            toDelete.Add(evt);
        foreach (var evt in toDelete)
            await _repository.DeleteAsync(evt.Id, tenantId, null, cancellationToken).ConfigureAwait(false);
        return toDelete.Count;
    }
}

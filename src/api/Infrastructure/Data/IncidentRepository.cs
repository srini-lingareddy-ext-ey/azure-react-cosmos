using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed incident repository (WO-64). Partition key /tenantId.</summary>
public sealed class IncidentRepository : IIncidentRepository
{
    private readonly IRepository<IncidentRecord> _repository;
    public IncidentRepository(IRepository<IncidentRecord> repository) { _repository = repository; }

    public Task<IncidentRecord?> GetByIdAsync(string id, string tenantId, CancellationToken ct = default) =>
        _repository.GetByIdAsync(id, tenantId, ct);

    public IAsyncEnumerable<IncidentRecord> GetByTenantAsync(string tenantId, string? severity, string? state, DateTimeOffset? from, DateTimeOffset? to, string? sort, string? order, int limit = 50, int offset = 0, CancellationToken ct = default)
    {
        var conditions = new List<string> { "c.tenantId = @tenantId" };
        var parameters = new Dictionary<string, object> { ["@tenantId"] = tenantId };
        if (!string.IsNullOrEmpty(severity)) { conditions.Add("c.severity = @severity"); parameters["@severity"] = Enum.Parse<IncidentSeverity>(severity, true); }
        if (!string.IsNullOrEmpty(state)) { conditions.Add("c.state = @state"); parameters["@state"] = Enum.Parse<IncidentState>(state, true); }
        if (from.HasValue) { conditions.Add("c.createdAt >= @from"); parameters["@from"] = from.Value; }
        if (to.HasValue) { conditions.Add("c.createdAt <= @to"); parameters["@to"] = to.Value; }
        var where = string.Join(" AND ", conditions);
        var sortField = sort?.ToLowerInvariant() switch { "severity" => "c.severity", "state" => "c.state", _ => "c.createdAt" };
        var sortDir = string.Equals(order, "asc", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC";
        var sql = $"SELECT * FROM c WHERE {where} ORDER BY {sortField} {sortDir} OFFSET @offset LIMIT @limit";
        parameters["@offset"] = offset;
        parameters["@limit"] = limit;
        return _repository.QueryAsync(new QuerySpec(sql, parameters), ct);
    }

    public async Task<int> CountByTenantAsync(string tenantId, string? severity, string? state, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
    {
        var conditions = new List<string> { "c.tenantId = @tenantId" };
        var parameters = new Dictionary<string, object> { ["@tenantId"] = tenantId };
        if (!string.IsNullOrEmpty(severity)) { conditions.Add("c.severity = @severity"); parameters["@severity"] = Enum.Parse<IncidentSeverity>(severity, true); }
        if (!string.IsNullOrEmpty(state)) { conditions.Add("c.state = @state"); parameters["@state"] = Enum.Parse<IncidentState>(state, true); }
        if (from.HasValue) { conditions.Add("c.createdAt >= @from"); parameters["@from"] = from.Value; }
        if (to.HasValue) { conditions.Add("c.createdAt <= @to"); parameters["@to"] = to.Value; }
        var where = string.Join(" AND ", conditions);
        var count = 0;
        await foreach (var _ in _repository.QueryAsync(new QuerySpec($"SELECT c.id FROM c WHERE {where}", parameters), ct).ConfigureAwait(false))
            count++;
        return count;
    }

    public Task<IncidentRecord> CreateAsync(IncidentRecord record, CancellationToken ct = default) =>
        _repository.CreateAsync(record, ct);

    public Task<IncidentRecord> UpdateAsync(IncidentRecord record, CancellationToken ct = default) =>
        _repository.UpdateAsync(record, ct);

    public async Task<IncidentRecord?> GetOpenByMonitorAsync(string monitorId, string tenantId, DateTimeOffset since, CancellationToken ct = default)
    {
        var spec = new QuerySpec(
            "SELECT * FROM c WHERE c.tenantId = @tenantId AND c.monitorId = @monitorId AND (c.state = @open OR c.state = @inProgress) AND c.createdAt >= @since ORDER BY c.createdAt DESC",
            new Dictionary<string, object>
            {
                ["@tenantId"] = tenantId,
                ["@monitorId"] = monitorId,
                ["@open"] = (int)IncidentState.Open,
                ["@inProgress"] = (int)IncidentState.InProgress,
                ["@since"] = since,
            });
        await foreach (var row in _repository.QueryAsync(spec, ct).ConfigureAwait(false))
            return row;
        return null;
    }
}

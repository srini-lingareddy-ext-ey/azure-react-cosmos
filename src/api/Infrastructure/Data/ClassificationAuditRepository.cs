using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class ClassificationAuditRepository : IClassificationAuditRepository
{
    private readonly IRepository<ClassificationAuditEntry> _repository;
    public ClassificationAuditRepository(IRepository<ClassificationAuditEntry> repository) { _repository = repository; }

    public IAsyncEnumerable<ClassificationAuditEntry> GetByTenantAsync(string tenantId, string? outcome, string? matchedRuleId, DateTimeOffset? from, DateTimeOffset? to, int limit = 50, int offset = 0, CancellationToken cancellationToken = default)
    {
        var conditions = new List<string> { "c.tenantId = @tenantId" };
        var parameters = new Dictionary<string, object> { ["@tenantId"] = tenantId };
        if (!string.IsNullOrEmpty(outcome)) { conditions.Add("c.outcome = @outcome"); parameters["@outcome"] = outcome; }
        if (!string.IsNullOrEmpty(matchedRuleId)) { conditions.Add("c.matchedRuleId = @matchedRuleId"); parameters["@matchedRuleId"] = matchedRuleId; }
        if (from.HasValue) { conditions.Add("c.classifiedAt >= @from"); parameters["@from"] = from.Value; }
        if (to.HasValue) { conditions.Add("c.classifiedAt <= @to"); parameters["@to"] = to.Value; }
        var where = string.Join(" AND ", conditions);
        var sql = $"SELECT * FROM c WHERE {where} ORDER BY c.classifiedAt DESC OFFSET @offset LIMIT @limit";
        parameters["@offset"] = offset;
        parameters["@limit"] = limit;
        return _repository.QueryAsync(new QuerySpec(sql, parameters), cancellationToken);
    }

    public Task<ClassificationAuditEntry> CreateAsync(ClassificationAuditEntry entry, CancellationToken cancellationToken = default) =>
        _repository.CreateAsync(entry, cancellationToken);
}

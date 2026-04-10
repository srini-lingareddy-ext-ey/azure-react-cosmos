using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class FingerprintAuditRepository : IFingerprintAuditRepository
{
    private readonly IRepository<FingerprintAuditEntry> _repository;
    public FingerprintAuditRepository(IRepository<FingerprintAuditEntry> repository) { _repository = repository; }

    public Task<FingerprintAuditEntry> CreateAsync(FingerprintAuditEntry entry, CancellationToken cancellationToken = default) =>
        _repository.CreateAsync(entry, cancellationToken);

    public IAsyncEnumerable<FingerprintAuditEntry> GetByTenantAsync(string tenantId, string? changeClassification, int limit = 50, int offset = 0, CancellationToken cancellationToken = default)
    {
        var conditions = new List<string> { "c.tenantId = @tenantId" };
        var parameters = new Dictionary<string, object> { ["@tenantId"] = tenantId };
        if (!string.IsNullOrEmpty(changeClassification)) { conditions.Add("c.changeClassification = @cc"); parameters["@cc"] = Enum.Parse<ChangeClassification>(changeClassification, true); }
        var where = string.Join(" AND ", conditions);
        var sql = $"SELECT * FROM c WHERE {where} ORDER BY c.detectedAt DESC OFFSET @offset LIMIT @limit";
        parameters["@offset"] = offset;
        parameters["@limit"] = limit;
        return _repository.QueryAsync(new QuerySpec(sql, parameters), cancellationToken);
    }

    public IAsyncEnumerable<FingerprintAuditEntry> GetByArtifactIdAsync(string artifactId, string tenantId, int limit = 30, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec($"SELECT TOP {limit} * FROM c WHERE c.tenantId = @tenantId AND c.artifactId = @artifactId ORDER BY c.detectedAt DESC",
            new Dictionary<string, object> { ["@tenantId"] = tenantId, ["@artifactId"] = artifactId });
        return _repository.QueryAsync(spec, cancellationToken);
    }
}

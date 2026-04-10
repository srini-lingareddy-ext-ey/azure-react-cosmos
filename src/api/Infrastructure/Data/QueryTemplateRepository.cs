using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed query template repository (WO-18). Partition key /tenantId.</summary>
public sealed class QueryTemplateRepository : IQueryTemplateRepository
{
    private readonly IRepository<QueryTemplate> _repository;
    public QueryTemplateRepository(IRepository<QueryTemplate> repository) { _repository = repository; }

    public Task<QueryTemplate?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, tenantId, cancellationToken);

    public IAsyncEnumerable<QueryTemplate> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public IAsyncEnumerable<QueryTemplate> GetByConnectorTypeAsync(string connectorTypeId, string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.connectorTypeId = @connectorTypeId AND c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@connectorTypeId"] = connectorTypeId, ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public async Task<QueryTemplate> CreateAsync(QueryTemplate template, CancellationToken cancellationToken = default)
    {
        if (template.SchemaVersion == 0) template.SchemaVersion = 1;
        return await _repository.CreateAsync(template, cancellationToken).ConfigureAwait(false);
    }

    public Task<QueryTemplate> UpdateAsync(QueryTemplate template, CancellationToken cancellationToken = default) =>
        _repository.UpdateAsync(template, cancellationToken);
}

using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

/// <summary>Persistence for <see cref="QueryTemplate"/> (WO-18).</summary>
public interface IQueryTemplateRepository
{
    Task<QueryTemplate?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<QueryTemplate> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<QueryTemplate> GetByConnectorTypeAsync(string connectorTypeId, string tenantId, CancellationToken cancellationToken = default);
    Task<QueryTemplate> CreateAsync(QueryTemplate template, CancellationToken cancellationToken = default);
    Task<QueryTemplate> UpdateAsync(QueryTemplate template, CancellationToken cancellationToken = default);
}

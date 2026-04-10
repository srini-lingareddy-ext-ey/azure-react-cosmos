using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

/// <summary>Persistence for <see cref="Connection"/> (WO-18).</summary>
public interface IConnectionRepository
{
    Task<Connection?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    Task<Connection?> GetByNameAsync(string name, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<Connection> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<Connection> CreateAsync(Connection connection, CancellationToken cancellationToken = default);
    Task<Connection> UpdateAsync(Connection connection, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, string tenantId, string? etag = null, CancellationToken cancellationToken = default);
}

using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

/// <summary>
/// Tenant persistence (WO-4). Implemented in Infrastructure; no Cosmos types here.
/// </summary>
public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a tenant by unique <see cref="Tenant.Name"/>. Uses a cross-partition query (Cosmos SQL).
    /// </summary>
    Task<Tenant?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    IAsyncEnumerable<Tenant> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Tenant> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default);

    Task<Tenant> UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
}

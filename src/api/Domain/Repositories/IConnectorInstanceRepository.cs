using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

/// <summary>Persistence for <see cref="ConnectorInstance"/> (WO-20).</summary>
public interface IConnectorInstanceRepository
{
    Task<ConnectorInstance?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ConnectorInstance> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ConnectorInstance> GetAllEnabledAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<ConnectorInstance> CreateAsync(ConnectorInstance instance, CancellationToken cancellationToken = default);
    Task<ConnectorInstance> UpdateAsync(ConnectorInstance instance, CancellationToken cancellationToken = default);
}

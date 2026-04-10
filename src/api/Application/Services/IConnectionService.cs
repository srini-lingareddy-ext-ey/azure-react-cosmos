using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

/// <summary>WO-45: connection CRUD, test, and referential integrity.</summary>
public interface IConnectionService
{
    Task<ConnectionListResponse> ListAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<ConnectionResponse> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    Task<ConnectionResponse> CreateAsync(string userId, string tenantId, CreateConnectionRequest request, CancellationToken cancellationToken = default);
    Task<ConnectionResponse> UpdateAsync(string userId, string id, string tenantId, UpdateConnectionRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    Task<ConnectionTestResponse> TestAsync(string id, string tenantId, CancellationToken cancellationToken = default);
}

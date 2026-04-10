using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

/// <summary>WO-47: connector instance CRUD, enable/disable, test, and logs.</summary>
public interface IConnectorService
{
    Task<ConnectorListResponse> ListAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<ConnectorResponse> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    Task<ConnectorResponse> CreateAsync(string userId, string tenantId, CreateConnectorRequest request, CancellationToken cancellationToken = default);
    Task<ConnectorResponse> UpdateAsync(string userId, string id, string tenantId, UpdateConnectorRequest request, CancellationToken cancellationToken = default);
    Task<ConnectorResponse> EnableAsync(string userId, string id, string tenantId, CancellationToken cancellationToken = default);
    Task<ConnectorResponse> DisableAsync(string userId, string id, string tenantId, CancellationToken cancellationToken = default);
    Task<ConnectorTestResponse> TestAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    Task<ConnectorLogResponse> GetLogsAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    IReadOnlyList<ConnectorTypeCatalogEntryDto> GetCatalog();
}

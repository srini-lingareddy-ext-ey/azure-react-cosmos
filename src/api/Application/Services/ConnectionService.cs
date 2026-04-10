using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Monitor = Todo.Api.Domain.Entities.Monitor;

namespace Todo.Api.Application.Services;

/// <summary>WO-45: connection CRUD with credential encryption, referential integrity on delete, and test.</summary>
public sealed class ConnectionService : IConnectionService
{
    private readonly IConnectionRepository _connectionRepository;
    private readonly IMonitorRepository _monitorRepository;
    private readonly ICredentialEncryptionService _encryptionService;

    public ConnectionService(
        IConnectionRepository connectionRepository,
        IMonitorRepository monitorRepository,
        ICredentialEncryptionService encryptionService)
    {
        _connectionRepository = connectionRepository;
        _monitorRepository = monitorRepository;
        _encryptionService = encryptionService;
    }

    public async Task<ConnectionListResponse> ListAsync(
        string tenantId, CancellationToken cancellationToken = default)
    {
        var all = new List<Connection>();
        await foreach (var c in _connectionRepository.GetAllByTenantAsync(tenantId, cancellationToken).ConfigureAwait(false))
        {
            all.Add(c);
        }

        return new ConnectionListResponse
        {
            Items = all.Select(MapToResponse).ToList(),
            TotalCount = all.Count,
        };
    }

    public async Task<ConnectionResponse> GetByIdAsync(
        string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var conn = await _connectionRepository.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (conn is null) throw new KeyNotFoundException("Connection not found.");
        return MapToResponse(conn);
    }

    public async Task<ConnectionResponse> CreateAsync(
        string userId, string tenantId, CreateConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var encrypted = await _encryptionService.EncryptAsync(request.Credentials, tenantId).ConfigureAwait(false);

        var entity = new Connection
        {
            Id = Guid.NewGuid().ToString("n"),
            TenantId = tenantId,
            ConnectionName = request.ConnectionName.Trim(),
            ConnectorTypeId = request.ConnectorTypeId.Trim(),
            IsEnabled = true,
            CredentialsEncrypted = encrypted,
            SchemaVersion = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId,
        };

        var created = await _connectionRepository.CreateAsync(entity, cancellationToken).ConfigureAwait(false);
        return MapToResponse(created);
    }

    public async Task<ConnectionResponse> UpdateAsync(
        string userId, string id, string tenantId, UpdateConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var conn = await _connectionRepository.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (conn is null) throw new KeyNotFoundException("Connection not found.");

        if (request.ConnectionName is not null) conn.ConnectionName = request.ConnectionName.Trim();
        if (request.ConnectorTypeId is not null) conn.ConnectorTypeId = request.ConnectorTypeId.Trim();
        if (request.IsEnabled.HasValue) conn.IsEnabled = request.IsEnabled.Value;
        if (request.Credentials is not null)
        {
            conn.CredentialsEncrypted = await _encryptionService.EncryptAsync(request.Credentials, tenantId).ConfigureAwait(false);
        }

        conn.UpdatedAt = DateTimeOffset.UtcNow;
        conn.UpdatedBy = userId;

        var updated = await _connectionRepository.UpdateAsync(conn, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    public async Task DeleteAsync(
        string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var conn = await _connectionRepository.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (conn is null) throw new KeyNotFoundException("Connection not found.");

        var dependentMonitors = new List<string>();
        await foreach (var m in _monitorRepository.GetByConnectionAsync(id, tenantId, cancellationToken).ConfigureAwait(false))
        {
            dependentMonitors.Add(m.MonitorName);
        }

        if (dependentMonitors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot delete connection '{conn.ConnectionName}' because it is referenced by monitors: {string.Join(", ", dependentMonitors)}");
        }

        await _connectionRepository.DeleteAsync(id, tenantId, conn.Etag, cancellationToken).ConfigureAwait(false);
    }

    public async Task<ConnectionTestResponse> TestAsync(
        string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var conn = await _connectionRepository.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (conn is null) throw new KeyNotFoundException("Connection not found.");

        try
        {
            await _encryptionService.DecryptAsync(conn.CredentialsEncrypted, tenantId).ConfigureAwait(false);

            conn.LastTestedAt = DateTimeOffset.UtcNow;
            conn.LastTestResult = "Success";
            await _connectionRepository.UpdateAsync(conn, cancellationToken).ConfigureAwait(false);

            return new ConnectionTestResponse { Success = true, Message = "Connection test passed." };
        }
        catch (Exception ex)
        {
            conn.LastTestedAt = DateTimeOffset.UtcNow;
            conn.LastTestResult = $"Failed: {ex.Message}";
            try
            {
                await _connectionRepository.UpdateAsync(conn, cancellationToken).ConfigureAwait(false);
            }
            catch
            {
                // Best-effort update of test result
            }

            return new ConnectionTestResponse { Success = false, Message = ex.Message };
        }
    }

    internal static ConnectionResponse MapToResponse(Connection c)
    {
        return new ConnectionResponse
        {
            Id = c.Id,
            TenantId = c.TenantId,
            ConnectionName = c.ConnectionName,
            ConnectorTypeId = c.ConnectorTypeId,
            IsEnabled = c.IsEnabled,
            LastTestedAt = c.LastTestedAt,
            LastTestResult = c.LastTestResult,
            SchemaVersion = c.SchemaVersion,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            CreatedBy = c.CreatedBy,
            UpdatedBy = c.UpdatedBy,
        };
    }
}

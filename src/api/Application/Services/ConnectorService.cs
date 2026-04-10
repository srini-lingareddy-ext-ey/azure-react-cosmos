using Todo.Api.Application.Connectors;
using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>WO-47: connector instance CRUD with credential encryption, test, and execution logs.</summary>
public sealed class ConnectorService : IConnectorService
{
    private readonly IConnectorInstanceRepository _connectorRepo;
    private readonly IConnectorExecutionLogRepository _logRepo;
    private readonly ICredentialEncryptionService _encryptionService;
    private readonly ConnectorTypeCatalog _catalog;

    public ConnectorService(
        IConnectorInstanceRepository connectorRepo,
        IConnectorExecutionLogRepository logRepo,
        ICredentialEncryptionService encryptionService,
        ConnectorTypeCatalog catalog)
    {
        _connectorRepo = connectorRepo;
        _logRepo = logRepo;
        _encryptionService = encryptionService;
        _catalog = catalog;
    }

    public async Task<ConnectorListResponse> ListAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var items = new List<ConnectorInstance>();
        await foreach (var c in _connectorRepo.GetAllByTenantAsync(tenantId, cancellationToken).ConfigureAwait(false))
            items.Add(c);

        return new ConnectorListResponse
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = items.Count,
        };
    }

    public async Task<ConnectorResponse> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var c = await _connectorRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (c is null) throw new KeyNotFoundException("Connector instance not found.");
        return MapToResponse(c);
    }

    public async Task<ConnectorResponse> CreateAsync(
        string userId, string tenantId, CreateConnectorRequest request, CancellationToken cancellationToken = default)
    {
        var catalogEntry = _catalog.GetById(request.ConnectorTypeId);
        if (catalogEntry is null)
            throw new InvalidOperationException("Unknown connector type.");

        var encrypted = await _encryptionService.EncryptAsync(request.Credentials, tenantId).ConfigureAwait(false);

        var entity = new ConnectorInstance
        {
            Id = Guid.NewGuid().ToString("n"),
            TenantId = tenantId,
            ConnectorName = request.ConnectorName.Trim(),
            ConnectorTypeId = request.ConnectorTypeId,
            IsEnabled = true,
            IntegrationMode = request.IntegrationMode,
            PollingScheduleCron = request.PollingScheduleCron,
            CredentialsEncrypted = encrypted,
            FieldMappings = MapFieldMappingsToEntity(request.FieldMappings),
            SchemaVersion = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId,
        };

        var created = await _connectorRepo.CreateAsync(entity, cancellationToken).ConfigureAwait(false);
        return MapToResponse(created);
    }

    public async Task<ConnectorResponse> UpdateAsync(
        string userId, string id, string tenantId, UpdateConnectorRequest request, CancellationToken cancellationToken = default)
    {
        var c = await _connectorRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (c is null) throw new KeyNotFoundException("Connector instance not found.");

        if (request.ConnectorName is not null) c.ConnectorName = request.ConnectorName.Trim();
        if (request.PollingScheduleCron is not null) c.PollingScheduleCron = request.PollingScheduleCron;
        if (request.IsEnabled.HasValue) c.IsEnabled = request.IsEnabled.Value;
        if (request.FieldMappings is not null) c.FieldMappings = MapFieldMappingsToEntity(request.FieldMappings);
        if (request.Credentials is not null)
            c.CredentialsEncrypted = await _encryptionService.EncryptAsync(request.Credentials, tenantId).ConfigureAwait(false);

        c.UpdatedAt = DateTimeOffset.UtcNow;
        c.UpdatedBy = userId;
        var updated = await _connectorRepo.UpdateAsync(c, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    public async Task<ConnectorResponse> EnableAsync(string userId, string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var c = await _connectorRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (c is null) throw new KeyNotFoundException("Connector instance not found.");
        c.IsEnabled = true;
        c.UpdatedAt = DateTimeOffset.UtcNow;
        c.UpdatedBy = userId;
        var updated = await _connectorRepo.UpdateAsync(c, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    public async Task<ConnectorResponse> DisableAsync(string userId, string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var c = await _connectorRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (c is null) throw new KeyNotFoundException("Connector instance not found.");
        c.IsEnabled = false;
        c.UpdatedAt = DateTimeOffset.UtcNow;
        c.UpdatedBy = userId;
        var updated = await _connectorRepo.UpdateAsync(c, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    public async Task<ConnectorTestResponse> TestAsync(string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var c = await _connectorRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (c is null) throw new KeyNotFoundException("Connector instance not found.");

        try
        {
            await _encryptionService.DecryptAsync(c.CredentialsEncrypted, tenantId).ConfigureAwait(false);
            return new ConnectorTestResponse { Success = true };
        }
        catch (Exception ex)
        {
            return new ConnectorTestResponse { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ConnectorLogResponse> GetLogsAsync(string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var c = await _connectorRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (c is null) throw new KeyNotFoundException("Connector instance not found.");

        var logs = await _logRepo.GetByConnectorIdAsync(id, tenantId, 30, cancellationToken).ConfigureAwait(false);
        var successCount = logs.Count(l => l.Status == ExecutionStatus.Success);
        var rate = logs.Count > 0 ? (double)successCount / logs.Count * 100.0 : 0.0;

        return new ConnectorLogResponse
        {
            Entries = logs.Select(l => new ConnectorLogEntryDto
            {
                Id = l.Id,
                ExecutedAt = l.ExecutedAt,
                Status = l.Status,
                EventsProduced = l.EventsProduced,
                DurationMs = l.DurationMs,
                ErrorMessage = l.ErrorMessage,
            }).ToList(),
            SuccessRateLast30Cycles = Math.Round(rate, 1),
        };
    }

    public IReadOnlyList<ConnectorTypeCatalogEntryDto> GetCatalog()
    {
        return _catalog.GetAll().Select(e => new ConnectorTypeCatalogEntryDto
        {
            ConnectorTypeId = e.ConnectorTypeId,
            DisplayName = e.DisplayName,
            IntegrationMode = e.IntegrationMode,
            CertificationStatus = e.CertificationStatus,
            RequiredCredentialFields = e.RequiredCredentialFields,
        }).ToList();
    }

    private static List<ConnectorInstance.FieldMapping> MapFieldMappingsToEntity(List<FieldMappingDto>? dtos)
    {
        if (dtos is null or { Count: 0 }) return new List<ConnectorInstance.FieldMapping>();
        return dtos.Select(d => new ConnectorInstance.FieldMapping
        {
            SourceField = d.SourceField,
            TargetField = d.TargetField,
            TransformType = d.TransformType,
            ValueMap = d.ValueMap,
        }).ToList();
    }

    internal static ConnectorResponse MapToResponse(ConnectorInstance c)
    {
        return new ConnectorResponse
        {
            Id = c.Id,
            TenantId = c.TenantId,
            ConnectorName = c.ConnectorName,
            ConnectorTypeId = c.ConnectorTypeId,
            IsEnabled = c.IsEnabled,
            IntegrationMode = c.IntegrationMode,
            PollingScheduleCron = c.PollingScheduleCron,
            FieldMappings = c.FieldMappings.Select(f => new FieldMappingDto
            {
                SourceField = f.SourceField,
                TargetField = f.TargetField,
                TransformType = f.TransformType,
                ValueMap = f.ValueMap,
            }).ToList(),
            SchemaVersion = c.SchemaVersion,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt,
            CreatedBy = c.CreatedBy,
            UpdatedBy = c.UpdatedBy,
        };
    }
}

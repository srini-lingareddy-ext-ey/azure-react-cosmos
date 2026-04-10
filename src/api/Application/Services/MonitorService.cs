using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Monitor = Todo.Api.Domain.Entities.Monitor;

namespace Todo.Api.Application.Services;

/// <summary>WO-46: monitor CRUD with pause/activate and query template snapshot.</summary>
public sealed class MonitorService : IMonitorService
{
    private readonly IMonitorRepository _monitorRepo;
    private readonly IPipelineRegistrationRepository _pipelineRepo;
    private readonly IConnectionRepository _connectionRepo;
    private readonly IBusinessPlanRepository _businessPlanRepo;
    private readonly IQueryTemplateRepository _queryTemplateRepo;

    public MonitorService(
        IMonitorRepository monitorRepo,
        IPipelineRegistrationRepository pipelineRepo,
        IConnectionRepository connectionRepo,
        IBusinessPlanRepository businessPlanRepo,
        IQueryTemplateRepository queryTemplateRepo)
    {
        _monitorRepo = monitorRepo;
        _pipelineRepo = pipelineRepo;
        _connectionRepo = connectionRepo;
        _businessPlanRepo = businessPlanRepo;
        _queryTemplateRepo = queryTemplateRepo;
    }

    public async Task<MonitorListResponse> ListAsync(
        string tenantId, string? status, string? businessPlanId,
        CancellationToken cancellationToken = default)
    {
        MonitorState? stateFilter = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<MonitorState>(status, true, out var parsed))
            stateFilter = parsed;

        var source = string.IsNullOrEmpty(businessPlanId)
            ? _monitorRepo.GetAllByTenantAsync(tenantId, cancellationToken)
            : _monitorRepo.GetByBusinessPlanAsync(businessPlanId, tenantId, cancellationToken);

        var items = new List<Monitor>();
        await foreach (var m in source.ConfigureAwait(false))
        {
            if (stateFilter.HasValue && m.Status != stateFilter.Value) continue;
            items.Add(m);
        }

        return new MonitorListResponse
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = items.Count,
        };
    }

    public async Task<MonitorResponse> GetByIdAsync(
        string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var m = await _monitorRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (m is null) throw new KeyNotFoundException("Monitor not found.");
        return MapToResponse(m);
    }

    public async Task<MonitorResponse> CreateAsync(
        string userId, string tenantId, CreateMonitorRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.PollingFrequencyMinutes < 1)
            throw new InvalidOperationException("Polling frequency must be at least 1 minute.");

        // Validate pipeline exists and is active
        var pipeline = await _pipelineRepo.GetByIdAsync(request.EntityId, tenantId, cancellationToken).ConfigureAwait(false);
        if (request.EntityType == MonitorEntityType.Pipeline && (pipeline is null || !pipeline.IsActive))
            throw new InvalidOperationException("Pipeline does not exist or is inactive.");

        // Validate connection exists
        var connection = await _connectionRepo.GetByIdAsync(request.ConnectionId, tenantId, cancellationToken).ConfigureAwait(false);
        if (connection is null)
            throw new InvalidOperationException("Connection not found in this tenant.");

        string? businessPlanName = null;
        if (!string.IsNullOrEmpty(request.BusinessPlanId))
        {
            var bp = await _businessPlanRepo.GetByIdAsync(request.BusinessPlanId, tenantId, cancellationToken).ConfigureAwait(false);
            if (bp is not null) businessPlanName = bp.Name;
        }

        string? queryTemplateSnapshot = null;
        if (!string.IsNullOrEmpty(request.QueryTemplateId))
        {
            var qt = await _queryTemplateRepo.GetByIdAsync(request.QueryTemplateId, tenantId, cancellationToken).ConfigureAwait(false);
            if (qt is not null) queryTemplateSnapshot = qt.TemplateBody;
        }

        var entity = new Monitor
        {
            Id = Guid.NewGuid().ToString("n"),
            TenantId = tenantId,
            MonitorName = request.MonitorName.Trim(),
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            EntityName = request.EntityName.Trim(),
            BusinessPlanId = request.BusinessPlanId,
            BusinessPlanName = businessPlanName,
            ConnectionId = request.ConnectionId,
            ConnectionName = connection.ConnectionName,
            QueryTemplateId = request.QueryTemplateId,
            QueryTemplateSnapshot = queryTemplateSnapshot,
            PollingFrequencyMinutes = request.PollingFrequencyMinutes,
            AlertThresholds = MapThresholdsToEntity(request.AlertThresholds),
            Status = MonitorState.Active,
            SchemaVersion = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId,
        };

        var created = await _monitorRepo.CreateAsync(entity, cancellationToken).ConfigureAwait(false);
        return MapToResponse(created);
    }

    public async Task<MonitorResponse> UpdateAsync(
        string userId, string id, string tenantId, UpdateMonitorRequest request,
        CancellationToken cancellationToken = default)
    {
        var m = await _monitorRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (m is null) throw new KeyNotFoundException("Monitor not found.");

        if (request.MonitorName is not null) m.MonitorName = request.MonitorName.Trim();
        if (request.PollingFrequencyMinutes.HasValue) m.PollingFrequencyMinutes = request.PollingFrequencyMinutes.Value;
        if (request.AlertThresholds is not null) m.AlertThresholds = MapThresholdsToEntity(request.AlertThresholds);

        if (request.QueryTemplateId is not null)
        {
            m.QueryTemplateId = request.QueryTemplateId;
            var qt = await _queryTemplateRepo.GetByIdAsync(request.QueryTemplateId, tenantId, cancellationToken).ConfigureAwait(false);
            m.QueryTemplateSnapshot = qt?.TemplateBody;
        }

        m.UpdatedAt = DateTimeOffset.UtcNow;
        m.UpdatedBy = userId;

        var updated = await _monitorRepo.UpdateAsync(m, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    public async Task<MonitorResponse> PauseAsync(
        string userId, string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var m = await _monitorRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (m is null) throw new KeyNotFoundException("Monitor not found.");

        m.Status = MonitorState.Paused;
        m.UpdatedAt = DateTimeOffset.UtcNow;
        m.UpdatedBy = userId;

        var updated = await _monitorRepo.UpdateAsync(m, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    public async Task<MonitorResponse> ActivateAsync(
        string userId, string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var m = await _monitorRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (m is null) throw new KeyNotFoundException("Monitor not found.");

        m.Status = MonitorState.Active;
        m.UpdatedAt = DateTimeOffset.UtcNow;
        m.UpdatedBy = userId;

        var updated = await _monitorRepo.UpdateAsync(m, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    private static List<Monitor.AlertThreshold> MapThresholdsToEntity(List<AlertThresholdDto>? dtos)
    {
        if (dtos is null or { Count: 0 }) return new List<Monitor.AlertThreshold>();
        return dtos.Select(d => new Monitor.AlertThreshold
        {
            MetricName = d.MetricName,
            WarningValue = d.WarningValue,
            CriticalValue = d.CriticalValue,
            Operator = d.Operator,
            Unit = d.Unit,
        }).ToList();
    }

    private static List<AlertThresholdDto>? MapThresholdsToDto(List<Monitor.AlertThreshold>? thresholds)
    {
        if (thresholds is null or { Count: 0 }) return null;
        return thresholds.Select(t => new AlertThresholdDto
        {
            MetricName = t.MetricName,
            WarningValue = t.WarningValue,
            CriticalValue = t.CriticalValue,
            Operator = t.Operator,
            Unit = t.Unit,
        }).ToList();
    }

    internal static MonitorResponse MapToResponse(Monitor m)
    {
        return new MonitorResponse
        {
            Id = m.Id,
            TenantId = m.TenantId,
            MonitorName = m.MonitorName,
            EntityType = m.EntityType,
            EntityId = m.EntityId,
            EntityName = m.EntityName,
            BusinessPlanId = m.BusinessPlanId,
            BusinessPlanName = m.BusinessPlanName,
            ConnectionId = m.ConnectionId,
            ConnectionName = m.ConnectionName,
            QueryTemplateId = m.QueryTemplateId,
            QueryTemplateSnapshot = m.QueryTemplateSnapshot,
            PollingFrequencyMinutes = m.PollingFrequencyMinutes,
            AlertThresholds = MapThresholdsToDto(m.AlertThresholds),
            Status = m.Status,
            SchemaVersion = m.SchemaVersion,
            CreatedAt = m.CreatedAt,
            UpdatedAt = m.UpdatedAt,
            CreatedBy = m.CreatedBy,
            UpdatedBy = m.UpdatedBy,
        };
    }
}

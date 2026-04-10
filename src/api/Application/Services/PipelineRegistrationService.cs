using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Monitor = Todo.Api.Domain.Entities.Monitor;

namespace Todo.Api.Application.Services;

/// <summary>WO-43: pipeline registration CRUD with deactivation/monitor suspension.</summary>
public sealed class PipelineRegistrationService : IPipelineRegistrationService
{
    private readonly IPipelineRegistrationRepository _pipelineRepo;
    private readonly IBusinessPlanRepository _businessPlanRepo;
    private readonly IMonitorRepository _monitorRepo;

    public PipelineRegistrationService(
        IPipelineRegistrationRepository pipelineRepo,
        IBusinessPlanRepository businessPlanRepo,
        IMonitorRepository monitorRepo)
    {
        _pipelineRepo = pipelineRepo;
        _businessPlanRepo = businessPlanRepo;
        _monitorRepo = monitorRepo;
    }

    public async Task<PipelineRegistrationListResponse> ListAsync(
        string tenantId, string? businessPlanId, string? medallionLayer,
        CancellationToken cancellationToken = default)
    {
        MedallionLayer? layerFilter = null;
        if (!string.IsNullOrEmpty(medallionLayer) && Enum.TryParse<MedallionLayer>(medallionLayer, true, out var parsed))
            layerFilter = parsed;

        var source = string.IsNullOrEmpty(businessPlanId)
            ? _pipelineRepo.GetAllByTenantAsync(tenantId, cancellationToken)
            : _pipelineRepo.GetByBusinessPlanAsync(businessPlanId, tenantId, cancellationToken);

        var items = new List<PipelineRegistration>();
        await foreach (var p in source.ConfigureAwait(false))
        {
            if (layerFilter.HasValue && p.MedallionLayer != layerFilter.Value) continue;
            items.Add(p);
        }

        return new PipelineRegistrationListResponse
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = items.Count,
        };
    }

    public async Task<PipelineRegistrationResponse> GetByIdAsync(
        string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var p = await _pipelineRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (p is null) throw new KeyNotFoundException("Pipeline registration not found.");
        return MapToResponse(p);
    }

    public async Task<PipelineRegistrationResponse> CreateAsync(
        string userId, string tenantId, CreatePipelineRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.PipelineName.Trim();
        var dup = await _pipelineRepo.GetByNameAsync(name, tenantId, cancellationToken).ConfigureAwait(false);
        if (dup is not null)
            throw new InvalidOperationException("A pipeline with this name already exists in the tenant.");

        string? businessPlanName = null;
        if (!string.IsNullOrEmpty(request.BusinessPlanId))
        {
            var bp = await _businessPlanRepo.GetByIdAsync(request.BusinessPlanId, tenantId, cancellationToken).ConfigureAwait(false);
            if (bp is null || !bp.IsActive)
                throw new InvalidOperationException("Business plan does not exist or is inactive.");
            businessPlanName = bp.Name;
        }

        var entity = new PipelineRegistration
        {
            Id = Guid.NewGuid().ToString("n"),
            TenantId = tenantId,
            PipelineName = name,
            SourceSystem = request.SourceSystem?.Trim(),
            TargetSystem = request.TargetSystem?.Trim(),
            MedallionLayer = request.MedallionLayer,
            BusinessPlanId = request.BusinessPlanId,
            BusinessPlanName = businessPlanName,
            Domain = request.Domain?.Trim(),
            Description = request.Description?.Trim(),
            IsActive = true,
            SchemaVersion = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId,
        };

        var created = await _pipelineRepo.CreateAsync(entity, cancellationToken).ConfigureAwait(false);
        return MapToResponse(created);
    }

    public async Task<PipelineRegistrationResponse> UpdateAsync(
        string userId, string id, string tenantId, UpdatePipelineRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        var p = await _pipelineRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (p is null) throw new KeyNotFoundException("Pipeline registration not found.");

        if (request.PipelineName is not null)
        {
            var trimmed = request.PipelineName.Trim();
            if (!string.Equals(p.PipelineName, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                var dup = await _pipelineRepo.GetByNameAsync(trimmed, tenantId, cancellationToken).ConfigureAwait(false);
                if (dup is not null)
                    throw new InvalidOperationException("A pipeline with this name already exists in the tenant.");
            }
            p.PipelineName = trimmed;
        }

        if (request.SourceSystem is not null) p.SourceSystem = request.SourceSystem.Trim();
        if (request.TargetSystem is not null) p.TargetSystem = request.TargetSystem.Trim();
        if (request.MedallionLayer.HasValue) p.MedallionLayer = request.MedallionLayer.Value;
        if (request.Domain is not null) p.Domain = request.Domain.Trim();
        if (request.Description is not null) p.Description = request.Description.Trim();

        if (request.BusinessPlanId is not null)
        {
            if (string.IsNullOrEmpty(request.BusinessPlanId))
            {
                p.BusinessPlanId = null;
                p.BusinessPlanName = null;
            }
            else
            {
                var bp = await _businessPlanRepo.GetByIdAsync(request.BusinessPlanId, tenantId, cancellationToken).ConfigureAwait(false);
                if (bp is null || !bp.IsActive)
                    throw new InvalidOperationException("Business plan does not exist or is inactive.");
                p.BusinessPlanId = request.BusinessPlanId;
                p.BusinessPlanName = bp.Name;
            }
        }

        p.UpdatedAt = DateTimeOffset.UtcNow;
        p.UpdatedBy = userId;

        var updated = await _pipelineRepo.UpdateAsync(p, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    public async Task<PipelineDeactivateResponse> DeactivateAsync(
        string userId, string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var p = await _pipelineRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (p is null) throw new KeyNotFoundException("Pipeline registration not found.");

        p.IsActive = false;
        p.UpdatedAt = DateTimeOffset.UtcNow;
        p.UpdatedBy = userId;
        var updated = await _pipelineRepo.UpdateAsync(p, cancellationToken).ConfigureAwait(false);

        var count = 0;
        await foreach (var m in _monitorRepo.GetByEntityAsync(id, tenantId, cancellationToken).ConfigureAwait(false))
        {
            if (m.Status == MonitorState.Active) count++;
        }

        await _monitorRepo.PauseByPipelineAsync(id, tenantId, cancellationToken).ConfigureAwait(false);

        return new PipelineDeactivateResponse
        {
            Pipeline = MapToResponse(updated),
            MonitorsSuspended = count,
        };
    }

    public async Task<PipelineRegistrationResponse> ActivateAsync(
        string userId, string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var p = await _pipelineRepo.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (p is null) throw new KeyNotFoundException("Pipeline registration not found.");

        p.IsActive = true;
        p.UpdatedAt = DateTimeOffset.UtcNow;
        p.UpdatedBy = userId;

        var updated = await _pipelineRepo.UpdateAsync(p, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    internal static PipelineRegistrationResponse MapToResponse(PipelineRegistration p)
    {
        return new PipelineRegistrationResponse
        {
            Id = p.Id,
            TenantId = p.TenantId,
            PipelineName = p.PipelineName,
            SourceSystem = p.SourceSystem,
            TargetSystem = p.TargetSystem,
            MedallionLayer = p.MedallionLayer,
            BusinessPlanId = p.BusinessPlanId,
            BusinessPlanName = p.BusinessPlanName,
            Domain = p.Domain,
            Description = p.Description,
            IsActive = p.IsActive,
            SchemaVersion = p.SchemaVersion,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            CreatedBy = p.CreatedBy,
            UpdatedBy = p.UpdatedBy,
        };
    }
}

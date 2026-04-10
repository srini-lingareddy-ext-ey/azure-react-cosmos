using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>WO-42: business plan CRUD with name-uniqueness and activate/deactivate lifecycle.</summary>
public sealed class BusinessPlanService : IBusinessPlanService
{
    private readonly IBusinessPlanRepository _repository;

    public BusinessPlanService(IBusinessPlanRepository repository)
    {
        _repository = repository;
    }

    public async Task<BusinessPlanListResponse> ListAsync(
        string tenantId, bool? isActive, CancellationToken cancellationToken = default)
    {
        var all = new List<BusinessPlan>();
        await foreach (var bp in _repository.GetAllByTenantAsync(tenantId, cancellationToken).ConfigureAwait(false))
        {
            if (isActive.HasValue && bp.IsActive != isActive.Value) continue;
            all.Add(bp);
        }

        return new BusinessPlanListResponse
        {
            Items = all.Select(MapToResponse).ToList(),
            TotalCount = all.Count,
        };
    }

    public async Task<BusinessPlanResponse> GetByIdAsync(
        string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var bp = await _repository.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (bp is null) throw new KeyNotFoundException("Business plan not found.");
        return MapToResponse(bp);
    }

    public async Task<BusinessPlanResponse> CreateAsync(
        string userId, string tenantId, CreateBusinessPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = request.Name.Trim();
        var duplicate = await _repository.GetByNameAsync(name, tenantId, cancellationToken).ConfigureAwait(false);
        if (duplicate is not null)
            throw new InvalidOperationException("A business plan with this name already exists in the tenant.");

        var entity = new BusinessPlan
        {
            Id = Guid.NewGuid().ToString("n"),
            TenantId = tenantId,
            Name = name,
            Description = request.Description?.Trim(),
            Domain = request.Domain?.Trim(),
            IsActive = true,
            DefaultSlaWindow = MapSlaWindow(request.DefaultSlaWindow),
            SchemaVersion = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId,
        };

        var created = await _repository.CreateAsync(entity, cancellationToken).ConfigureAwait(false);
        return MapToResponse(created);
    }

    public async Task<BusinessPlanResponse> UpdateAsync(
        string userId, string id, string tenantId, UpdateBusinessPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var bp = await _repository.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (bp is null) throw new KeyNotFoundException("Business plan not found.");

        if (request.Name is not null)
        {
            var trimmed = request.Name.Trim();
            if (!string.Equals(bp.Name, trimmed, StringComparison.OrdinalIgnoreCase))
            {
                var dup = await _repository.GetByNameAsync(trimmed, tenantId, cancellationToken).ConfigureAwait(false);
                if (dup is not null)
                    throw new InvalidOperationException("A business plan with this name already exists in the tenant.");
            }
            bp.Name = trimmed;
        }

        if (request.Description is not null) bp.Description = request.Description.Trim();
        if (request.Domain is not null) bp.Domain = request.Domain.Trim();
        if (request.DefaultSlaWindow is not null) bp.DefaultSlaWindow = MapSlaWindow(request.DefaultSlaWindow);

        bp.UpdatedAt = DateTimeOffset.UtcNow;
        bp.UpdatedBy = userId;

        var updated = await _repository.UpdateAsync(bp, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    public async Task<BusinessPlanResponse> ActivateAsync(
        string userId, string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var bp = await _repository.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (bp is null) throw new KeyNotFoundException("Business plan not found.");

        bp.IsActive = true;
        bp.UpdatedAt = DateTimeOffset.UtcNow;
        bp.UpdatedBy = userId;

        var updated = await _repository.UpdateAsync(bp, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    public async Task<BusinessPlanResponse> DeactivateAsync(
        string userId, string id, string tenantId, CancellationToken cancellationToken = default)
    {
        var bp = await _repository.GetByIdAsync(id, tenantId, cancellationToken).ConfigureAwait(false);
        if (bp is null) throw new KeyNotFoundException("Business plan not found.");

        bp.IsActive = false;
        bp.UpdatedAt = DateTimeOffset.UtcNow;
        bp.UpdatedBy = userId;

        var updated = await _repository.UpdateAsync(bp, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    private static BusinessPlan.SLAWindowConfig? MapSlaWindow(SLAWindowConfigDto? dto)
    {
        if (dto is null) return null;
        return new BusinessPlan.SLAWindowConfig
        {
            WindowType = dto.WindowType,
            WindowValue = dto.WindowValue,
            AtRiskBufferMinutes = dto.AtRiskBufferMinutes,
        };
    }

    private static SLAWindowConfigDto? MapSlaWindowDto(BusinessPlan.SLAWindowConfig? config)
    {
        if (config is null) return null;
        return new SLAWindowConfigDto
        {
            WindowType = config.WindowType,
            WindowValue = config.WindowValue,
            AtRiskBufferMinutes = config.AtRiskBufferMinutes,
        };
    }

    internal static BusinessPlanResponse MapToResponse(BusinessPlan bp)
    {
        return new BusinessPlanResponse
        {
            Id = bp.Id,
            TenantId = bp.TenantId,
            Name = bp.Name,
            Description = bp.Description,
            Domain = bp.Domain,
            IsActive = bp.IsActive,
            DefaultSlaWindow = MapSlaWindowDto(bp.DefaultSlaWindow),
            SchemaVersion = bp.SchemaVersion,
            CreatedAt = bp.CreatedAt,
            UpdatedAt = bp.UpdatedAt,
            CreatedBy = bp.CreatedBy,
            UpdatedBy = bp.UpdatedBy,
        };
    }
}

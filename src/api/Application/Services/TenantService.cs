using Microsoft.Extensions.Caching.Distributed;
using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Exceptions;
using Todo.Api.Domain.Repositories;
using Todo.Api.Infrastructure.Caching;

namespace Todo.Api.Application.Services;

/// <summary>
/// WO-9: tenant CRUD; authorization uses Cosmos role assignments (PlatformAdmin / per-tenant Admin).
/// </summary>
public sealed class TenantService : ITenantService
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    private readonly ITenantRepository _tenantRepository;
    private readonly IUserRoleAssignmentRepository _assignmentRepository;
    private readonly IDistributedCache _cache;

    public TenantService(
        ITenantRepository tenantRepository,
        IUserRoleAssignmentRepository assignmentRepository,
        IDistributedCache cache)
    {
        _tenantRepository = tenantRepository;
        _assignmentRepository = assignmentRepository;
        _cache = cache;
    }

    /// <inheritdoc />
    public async Task<TenantListResponse> ListTenantsAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await RequirePlatformAdminAsync(userId, cancellationToken).ConfigureAwait(false);

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize > 0 ? pageSize : DefaultPageSize, 1, MaxPageSize);

        var all = new List<Tenant>();
        await foreach (var t in _tenantRepository.GetAllAsync(cancellationToken).ConfigureAwait(false))
        {
            all.Add(t);
        }

        all.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
        var total = all.Count;
        var items = all
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MapToResponse)
            .ToList();

        return new TenantListResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
        };
    }

    /// <inheritdoc />
    public async Task<TenantResponse> CreateTenantAsync(
        string userId,
        CreateTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        await RequirePlatformAdminAsync(userId, cancellationToken).ConfigureAwait(false);

        var name = request.Name.Trim();
        var displayName = request.DisplayName.Trim();
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(displayName))
        {
            throw new ArgumentException("Name and DisplayName are required.");
        }

        var duplicate = await _tenantRepository.GetByNameAsync(name, cancellationToken).ConfigureAwait(false);
        if (duplicate is not null)
        {
            throw new TenantNameConflictException(name);
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid().ToString("n"),
            Name = name,
            DisplayName = displayName,
            Status = TenantStatus.Active,
            SchemaVersion = 1,
            Config = CreateDefaultTenantConfig(),
            Branding = BuildBranding(request),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId,
        };

        var created = await _tenantRepository.CreateAsync(tenant, cancellationToken).ConfigureAwait(false);
        await InvalidateTenantCacheAsync(created.Id, cancellationToken).ConfigureAwait(false);
        return MapToResponse(created);
    }

    /// <inheritdoc />
    public async Task<TenantResponse> GetTenantAsync(
        string userId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        await RequirePlatformAdminOrTenantAdminAsync(userId, tenantId, cancellationToken).ConfigureAwait(false);

        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            throw new KeyNotFoundException($"Tenant '{tenantId}' was not found.");
        }

        return MapToResponse(tenant);
    }

    /// <inheritdoc />
    public async Task<TenantResponse> PatchTenantConfigAsync(
        string userId,
        string tenantId,
        UpdateTenantConfigRequest request,
        CancellationToken cancellationToken = default)
    {
        await RequirePlatformAdminOrTenantAdminAsync(userId, tenantId, cancellationToken).ConfigureAwait(false);

        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            throw new KeyNotFoundException($"Tenant '{tenantId}' was not found.");
        }

        tenant.Config ??= new TenantConfig();
        if (request.HealthScoreWeights is not null)
        {
            tenant.Config.HealthScoreWeights ??= new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var kv in request.HealthScoreWeights)
            {
                tenant.Config.HealthScoreWeights[kv.Key] = kv.Value;
            }
        }

        if (request.HealthStatusThresholds is not null)
        {
            tenant.Config.HealthStatusThresholds ??= new HealthStatusThresholds();
            var p = request.HealthStatusThresholds;
            if (p.HealthyMin.HasValue)
            {
                tenant.Config.HealthStatusThresholds.HealthyMin = p.HealthyMin;
            }

            if (p.WarningMin.HasValue)
            {
                tenant.Config.HealthStatusThresholds.WarningMin = p.WarningMin;
            }

            if (p.CriticalBelow.HasValue)
            {
                tenant.Config.HealthStatusThresholds.CriticalBelow = p.CriticalBelow;
            }
        }

        ValidateMergedHealthScoreWeights(tenant.Config);

        tenant.UpdatedAt = DateTimeOffset.UtcNow;
        tenant.UpdatedBy = userId;

        var updated = await _tenantRepository.UpdateAsync(tenant, cancellationToken).ConfigureAwait(false);
        await InvalidateTenantCacheAsync(updated.Id, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    /// <inheritdoc />
    public async Task<TenantResponse> ActivateTenantAsync(
        string userId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        await RequirePlatformAdminAsync(userId, cancellationToken).ConfigureAwait(false);

        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            throw new KeyNotFoundException($"Tenant '{tenantId}' was not found.");
        }

        tenant.Status = TenantStatus.Active;
        tenant.UpdatedAt = DateTimeOffset.UtcNow;
        tenant.UpdatedBy = userId;

        var updated = await _tenantRepository.UpdateAsync(tenant, cancellationToken).ConfigureAwait(false);
        await InvalidateTenantCacheAsync(updated.Id, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    /// <inheritdoc />
    public async Task<TenantResponse> DeactivateTenantAsync(
        string userId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        await RequirePlatformAdminAsync(userId, cancellationToken).ConfigureAwait(false);

        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            throw new KeyNotFoundException($"Tenant '{tenantId}' was not found.");
        }

        tenant.Status = TenantStatus.Inactive;
        tenant.UpdatedAt = DateTimeOffset.UtcNow;
        tenant.UpdatedBy = userId;

        var updated = await _tenantRepository.UpdateAsync(tenant, cancellationToken).ConfigureAwait(false);
        await InvalidateTenantCacheAsync(updated.Id, cancellationToken).ConfigureAwait(false);
        return MapToResponse(updated);
    }

    private static TenantConfig CreateDefaultTenantConfig()
    {
        return new TenantConfig
        {
            HealthScoreWeights = new Dictionary<string, double>(StringComparer.Ordinal) { ["composite"] = 100 },
            HealthStatusThresholds = new HealthStatusThresholds
            {
                HealthyMin = 85,
                WarningMin = 60,
            },
        };
    }

    private static TenantBranding? BuildBranding(CreateTenantRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.LogoUrl) && string.IsNullOrWhiteSpace(request.BackgroundImageUrl))
        {
            return null;
        }

        return new TenantBranding
        {
            LogoUrl = string.IsNullOrWhiteSpace(request.LogoUrl) ? null : request.LogoUrl.Trim(),
            BackgroundImageUrl = string.IsNullOrWhiteSpace(request.BackgroundImageUrl) ? null : request.BackgroundImageUrl.Trim(),
        };
    }

    private async Task RequirePlatformAdminAsync(string userId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException();
        }

        if (!await IsPlatformAdminAsync(userId, cancellationToken).ConfigureAwait(false))
        {
            throw new UnauthorizedAccessException("This operation requires the PlatformAdmin role.");
        }
    }

    private async Task RequirePlatformAdminOrTenantAdminAsync(
        string userId,
        string tenantId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException();
        }

        if (await IsPlatformAdminAsync(userId, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var assignment = await _assignmentRepository
            .GetByUserAndTenantAsync(userId, tenantId, cancellationToken)
            .ConfigureAwait(false);
        if (assignment is null
            || assignment.Status != UserStatus.Active
            || assignment.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("This operation requires PlatformAdmin or Admin access to this tenant.");
        }
    }

    private async Task<bool> IsPlatformAdminAsync(string userId, CancellationToken cancellationToken)
    {
        await foreach (var a in _assignmentRepository.GetAllByUserAsync(userId, cancellationToken).ConfigureAwait(false))
        {
            if (a.Role == UserRole.PlatformAdmin && a.Status == UserStatus.Active)
            {
                return true;
            }
        }

        return false;
    }

    private Task InvalidateTenantCacheAsync(string tenantId, CancellationToken cancellationToken)
    {
        var key = TenantCacheKeys.ForTenant(tenantId);
        return _cache.RemoveAsync(key, cancellationToken);
    }

    /// <summary>
    /// When any weights are stored, they must sum to 100 (WO-9).
    /// </summary>
    internal static void ValidateMergedHealthScoreWeights(TenantConfig config)
    {
        if (config.HealthScoreWeights is not { Count: > 0 })
        {
            return;
        }

        var sum = config.HealthScoreWeights.Values.Sum();
        if (Math.Abs(sum - 100.0) > 0.0001)
        {
            throw new InvalidOperationException(
                $"Health score weights must sum to 100 after merge (current sum: {sum:0.###}).");
        }
    }

    internal static TenantResponse MapToResponse(Tenant t)
    {
        return new TenantResponse
        {
            Id = t.Id,
            Name = t.Name,
            DisplayName = t.DisplayName,
            Status = t.Status,
            SchemaVersion = t.SchemaVersion,
            CreatedAt = t.CreatedAt,
            UpdatedAt = t.UpdatedAt,
            Branding = t.Branding is null
                ? null
                : new TenantBrandingDto
                {
                    LogoUrl = t.Branding.LogoUrl,
                    BackgroundImageUrl = t.Branding.BackgroundImageUrl,
                },
            Config = t.Config is null
                ? null
                : new TenantConfigDto
                {
                    HealthScoreWeights = t.Config.HealthScoreWeights.ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value,
                        StringComparer.Ordinal),
                    HealthStatusThresholds = t.Config.HealthStatusThresholds is null
                        ? null
                        : new HealthStatusThresholdsDto
                        {
                            HealthyMin = t.Config.HealthStatusThresholds.HealthyMin,
                            WarningMin = t.Config.HealthStatusThresholds.WarningMin,
                            CriticalBelow = t.Config.HealthStatusThresholds.CriticalBelow,
                        },
                },
        };
    }
}

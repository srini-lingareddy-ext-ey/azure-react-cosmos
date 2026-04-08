using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

/// <summary>WO-9: tenant CRUD and config updates.</summary>
public interface ITenantService
{
    Task<TenantListResponse> ListTenantsAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<TenantResponse> CreateTenantAsync(
        string userId,
        CreateTenantRequest request,
        CancellationToken cancellationToken = default);

    Task<TenantResponse> GetTenantAsync(
        string userId,
        string tenantId,
        CancellationToken cancellationToken = default);

    Task<TenantResponse> PatchTenantConfigAsync(
        string userId,
        string tenantId,
        UpdateTenantConfigRequest request,
        CancellationToken cancellationToken = default);

    Task<TenantResponse> ActivateTenantAsync(
        string userId,
        string tenantId,
        CancellationToken cancellationToken = default);

    Task<TenantResponse> DeactivateTenantAsync(
        string userId,
        string tenantId,
        CancellationToken cancellationToken = default);
}

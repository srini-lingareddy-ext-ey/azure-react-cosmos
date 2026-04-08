using System.Net.Http;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Infrastructure.Configuration;

/// <summary>WO-9: placeholder when Cosmos is not configured so <see cref="ITenantService"/> can still be resolved.</summary>
public sealed class UnavailableTenantService : ITenantService
{
    private static readonly HttpRequestException Ex = new("The tenant API requires Azure Cosmos DB (AZURE_COSMOS_ENDPOINT) to be configured.");

    public Task<TenantListResponse> ListTenantsAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default) =>
        throw Ex;

    public Task<TenantResponse> CreateTenantAsync(
        string userId,
        CreateTenantRequest request,
        CancellationToken cancellationToken = default) =>
        throw Ex;

    public Task<TenantResponse> GetTenantAsync(
        string userId,
        string tenantId,
        CancellationToken cancellationToken = default) =>
        throw Ex;

    public Task<TenantResponse> PatchTenantConfigAsync(
        string userId,
        string tenantId,
        UpdateTenantConfigRequest request,
        CancellationToken cancellationToken = default) =>
        throw Ex;

    public Task<TenantResponse> ActivateTenantAsync(
        string userId,
        string tenantId,
        CancellationToken cancellationToken = default) =>
        throw Ex;

    public Task<TenantResponse> DeactivateTenantAsync(
        string userId,
        string tenantId,
        CancellationToken cancellationToken = default) =>
        throw Ex;
}

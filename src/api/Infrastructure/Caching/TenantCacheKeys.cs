namespace Todo.Api.Infrastructure.Caching;

/// <summary>Cache keys for tenant documents (WO-9; AC-FOUNDATION-010 cache namespace).</summary>
public static class TenantCacheKeys
{
    /// <summary>Invalidated after tenant config or status changes.</summary>
    public static string ForTenant(string tenantId) => $"cache:tenant:{tenantId}";
}

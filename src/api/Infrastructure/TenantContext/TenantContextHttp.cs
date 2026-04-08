namespace Todo.Api.Infrastructure.TenantContext;

/// <summary>HTTP surface for WO-6 tenant resolution (single header for this work order).</summary>
public static class TenantContextHttp
{
    public const string TenantIdHeaderName = "X-Tenant-Id";
}

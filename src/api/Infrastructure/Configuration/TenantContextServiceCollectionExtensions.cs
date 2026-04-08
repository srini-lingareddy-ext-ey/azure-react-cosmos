using Todo.Api.Application.Services;
using Todo.Api.Infrastructure.Identity;
using Todo.Api.Infrastructure.Middleware;

namespace Todo.Api.Infrastructure.Configuration;

/// <summary>WO-6 / WO-7: scoped tenant context and middleware registration.</summary>
public static class TenantContextServiceCollectionExtensions
{
    public static IServiceCollection AddTenantContext(this IServiceCollection services)
    {
        services.AddScoped<TenantContextService>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContextService>());
        services.AddScoped<ICurrentTenantContext>(sp => sp.GetRequiredService<TenantContextService>());
        services.AddScoped<TenantContextMiddleware>();
        return services;
    }
}

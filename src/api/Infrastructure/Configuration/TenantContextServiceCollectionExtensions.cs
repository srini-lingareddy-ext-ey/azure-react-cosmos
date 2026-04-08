using Todo.Api.Application.Services;
using Todo.Api.Infrastructure.Middleware;
using Todo.Api.Infrastructure.TenantContext;

namespace Todo.Api.Infrastructure.Configuration;

/// <summary>WO-6: scoped tenant context and middleware registration.</summary>
public static class TenantContextServiceCollectionExtensions
{
    public static IServiceCollection AddTenantContext(this IServiceCollection services)
    {
        services.AddScoped<CurrentTenantContext>();
        services.AddScoped<ICurrentTenantContext>(sp => sp.GetRequiredService<CurrentTenantContext>());
        services.AddScoped<TenantContextMiddleware>();
        return services;
    }
}

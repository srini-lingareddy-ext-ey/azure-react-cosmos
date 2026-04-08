using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Todo.Api.Infrastructure;

namespace Todo.Api.Infrastructure.Configuration;

/// <summary>
/// JWT bearer authentication with Microsoft Entra ID (AC-FOUNDATION-003).
/// Configures token validation (signature, expiration, issuer, audience) and standardized 401/403 responses.
/// </summary>
public static class AuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// Adds JWT bearer authentication using Entra ID authority and audience from configuration.
    /// Token validation checks signature, expiration, issuer, and audience. Authentication/authorization
    /// failures return JSON error body with traceId, errorCode, and message (AC-FOUNDATION-003.1–003.3, 003.6).
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtAuthenticationOptions>(configuration.GetSection(JwtAuthenticationOptions.SectionName));

        var section = configuration.GetSection(JwtAuthenticationOptions.SectionName);
        var authority = section["Authority"] ?? string.Empty;
        var audience = section["Audience"] ?? string.Empty;
        var requireHttpsMetadata = section.GetValue("RequireHttpsMetadata", true);

        // Single-tenant: Authority must be a tenant-specific URL (e.g. https://login.microsoftonline.com/{tenant-id}/v2.0).
        // Do not use "common" or "organizations"; issuer validation below enforces this by setting ValidIssuer to this authority.
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience = audience;
                options.RequireHttpsMetadata = requireHttpsMetadata;

                var validIssuer = string.IsNullOrEmpty(authority) ? null : authority.TrimEnd('/');
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    // Enforces single-tenant: only tokens issued by this tenant's authority are accepted (no common/organizations).
                    ValidIssuer = validIssuer,
                    ValidAudience = audience,
                    ClockSkew = TimeSpan.FromMinutes(2),
                };

                // Preserve short JWT claim names (e.g. oid, sub) so ICurrentUserService and WO-6 assignment UserId stay aligned with Entra v2 tokens.
                options.MapInboundClaims = false;

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";
                        var envelope = new ApiErrorEnvelope(
                            context.HttpContext.TraceIdentifier,
                            ErrorCodes.Unauthorized,
                            "Authentication required. Provide a valid JWT bearer token.");
                        var body = JsonSerializer.Serialize(envelope);
                        return context.Response.WriteAsync(body);
                    },
                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        var envelope = new ApiErrorEnvelope(
                            context.HttpContext.TraceIdentifier,
                            ErrorCodes.Forbidden,
                            "Insufficient permissions to access this resource.");
                        var body = JsonSerializer.Serialize(envelope);
                        return context.Response.WriteAsync(body);
                    },
                };
            });

        // Admin policy: requires Entra ID app role "Admin". The roles claim in the JWT must contain "Admin".
        // Configure the app registration in Entra ID with an app role named "Admin" and assign users/groups to it.
        services.AddAuthorizationBuilder()
            .AddPolicy("Admin", policy => policy.RequireRole("Admin"));

        return services;
    }
}

using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Todo.Api.Infrastructure.Identity;
using Todo.Api.Infrastructure.TenantContext;

namespace Todo.Api.Infrastructure.Middleware;

/// <summary>
/// For endpoints with <see cref="RequireTenantContextAttribute"/>: requires <c>X-Tenant-Id</c>, resolves tenant,
/// validates access (role assignment or PlatformAdmin bypass), and populates <see cref="ICurrentTenantContext"/>.
/// </summary>
public sealed class TenantContextMiddleware : IMiddleware
{
    private readonly ILogger<TenantContextMiddleware> _logger;

    public TenantContextMiddleware(ILogger<TenantContextMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (IsTenantResolutionExemptPath(context.Request.Path))
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<RequireTenantContextAttribute>() is null)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        var assignmentRepo = context.RequestServices.GetService<IUserRoleAssignmentRepository>();
        var tenantRepo = context.RequestServices.GetService<ITenantRepository>();
        if (assignmentRepo is null || tenantRepo is null)
        {
            _logger.LogWarning("Tenant context required but Cosmos repositories are not registered.");
            await WriteErrorAsync(
                    context,
                    StatusCodes.Status503ServiceUnavailable,
                    ErrorCodes.ServiceUnavailable,
                    "Tenant resolution is not available. Configure Cosmos DB for this environment.")
                .ConfigureAwait(false);
            return;
        }

        if (!context.Request.Headers.TryGetValue(TenantContextHttp.TenantIdHeaderName, out var tenantIdValues))
        {
            await WriteErrorAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    ErrorCodes.TenantAccessDenied,
                    $"Missing required header {TenantContextHttp.TenantIdHeaderName}.")
                .ConfigureAwait(false);
            return;
        }

        var tenantId = tenantIdValues.ToString().Trim();
        if (string.IsNullOrEmpty(tenantId))
        {
            await WriteErrorAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    ErrorCodes.TenantAccessDenied,
                    $"{TenantContextHttp.TenantIdHeaderName} must be a non-empty tenant id.")
                .ConfigureAwait(false);
            return;
        }

        var userService = context.RequestServices.GetRequiredService<ICurrentUserService>();
        var userId = userService.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            await WriteErrorAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    ErrorCodes.Forbidden,
                    "The authenticated token does not carry a stable user id (oid/sub) required for tenant access.")
                .ConfigureAwait(false);
            return;
        }

        var isPlatformAdmin = await HasPlatformAdminRoleAnywhereAsync(assignmentRepo, userId, context.RequestAborted)
            .ConfigureAwait(false);

        var tenant = await tenantRepo.GetByIdAsync(tenantId, context.RequestAborted).ConfigureAwait(false);
        if (tenant is null)
        {
            await WriteErrorAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    ErrorCodes.TenantAccessDenied,
                    "Tenant was not found or you do not have access.")
                .ConfigureAwait(false);
            return;
        }

        if (tenant.Status != TenantStatus.Active)
        {
            await WriteErrorAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    ErrorCodes.TenantInactive,
                    "This tenant is not active.")
                .ConfigureAwait(false);
            return;
        }

        if (isPlatformAdmin)
        {
            context.RequestServices.GetRequiredService<TenantContextService>()
                .Set(tenant.Id, tenant.Name, tenant.Status, UserRole.PlatformAdmin, UserStatus.Active);
            await next(context).ConfigureAwait(false);
            return;
        }

        var assignment = await assignmentRepo
            .GetByUserAndTenantAsync(userId, tenantId, context.RequestAborted)
            .ConfigureAwait(false);
        if (assignment is null)
        {
            await WriteErrorAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    ErrorCodes.TenantAccessDenied,
                    "You do not have access to this tenant.")
                .ConfigureAwait(false);
            return;
        }

        if (assignment.Status != UserStatus.Active)
        {
            await WriteErrorAsync(
                    context,
                    StatusCodes.Status403Forbidden,
                    ErrorCodes.TenantAccessDenied,
                    "Your access to this tenant is inactive.")
                .ConfigureAwait(false);
            return;
        }

        context.RequestServices.GetRequiredService<TenantContextService>()
            .Set(tenant.Id, tenant.Name, tenant.Status, assignment.Role, assignment.Status);

        await next(context).ConfigureAwait(false);
    }

    private static async Task<bool> HasPlatformAdminRoleAnywhereAsync(
        IUserRoleAssignmentRepository assignmentRepo,
        string userId,
        CancellationToken cancellationToken)
    {
        await foreach (var a in assignmentRepo.GetAllByUserAsync(userId, cancellationToken).ConfigureAwait(false))
        {
            if (a.Role == UserRole.PlatformAdmin)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Paths that must not require tenant resolution (WO-7).
    /// </summary>
    private static bool IsTenantResolutionExemptPath(PathString path)
    {
        var p = path.Value ?? string.Empty;
        if (string.Equals(p, "/api/v1/auth/me", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (p.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static Task WriteErrorAsync(HttpContext context, int statusCode, string errorCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var envelope = new ApiErrorEnvelope(context.TraceIdentifier, errorCode, message);
        var body = JsonSerializer.Serialize(envelope);
        return context.Response.WriteAsync(body);
    }
}

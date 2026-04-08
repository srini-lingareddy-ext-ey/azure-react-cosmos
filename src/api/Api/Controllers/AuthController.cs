using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;
using Todo.Api.Infrastructure;
using Todo.Api.Infrastructure.TenantContext;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-7 / WO-8: authenticated identity (<c>GET /api/v1/auth/me</c>).</summary>
[ApiController]
[Route("api/v1/auth")]
[Authorize]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Returns profile and tenant memberships. Does not require <c>X-Tenant-Id</c>; optional header selects <see cref="UserProfileResponse.ActiveTenant"/>.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetMe(
        [FromServices] ICurrentUserService users,
        CancellationToken cancellationToken)
    {
        var userId = users.UserId;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var preferredTenant = Request.Headers[TenantContextHttp.TenantIdHeaderName].ToString();
        if (string.IsNullOrWhiteSpace(preferredTenant))
        {
            preferredTenant = null;
        }

        var displayName = User.FindFirstValue("name")
            ?? User.FindFirstValue("given_name")
            ?? User.FindFirstValue("preferred_username");
        var email = User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("emails")
            ?? User.FindFirstValue("email");

        var profile = await _authService
            .GetCurrentUserProfileAsync(userId, preferredTenant, displayName, email, cancellationToken)
            .ConfigureAwait(false);

        if (profile is null)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new ApiErrorEnvelope(
                    HttpContext.TraceIdentifier,
                    ErrorCodes.UserNotProvisioned,
                    "The authenticated user has no active tenant role assignments."));
        }

        return Ok(profile);
    }
}

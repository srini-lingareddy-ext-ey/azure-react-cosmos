using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-10: tenant user roster and role/status (requires <c>X-Tenant-Id</c>).</summary>
[ApiController]
[Route("api/v1/tenants/{tenantId}/users")]
[Authorize]
[RequireTenantContext]
public sealed class UsersController : ControllerBase
{
    private readonly IUserManagementService _userManagement;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantContext _tenantContext;

    public UsersController(
        IUserManagementService userManagement,
        ICurrentUserService currentUser,
        ICurrentTenantContext tenantContext)
    {
        _userManagement = userManagement;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    /// <summary>Paginated roster (PlatformAdmin or tenant Admin).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(UserRosterResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserRosterResponse>> GetRosterAsync(
        [FromRoute] string tenantId,
        [FromQuery] UserRole? role,
        [FromQuery] UserStatus? status,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken cancellationToken = default)
    {
        if (!EnsureTenantRouteMatchesContext(tenantId, out var forbidden))
        {
            return forbidden;
        }

        var actorId = _currentUser.UserId ?? string.Empty;
        var result = await _userManagement
            .GetRosterAsync(actorId, tenantId, role, status, limit, offset, cancellationToken)
            .ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Change role (PlatformAdmin or tenant Admin; cannot change own role).</summary>
    [HttpPatch("{userId}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> PatchRoleAsync(
        [FromRoute] string tenantId,
        [FromRoute] string userId,
        [FromBody] ChangeRoleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!EnsureTenantRouteMatchesContext(tenantId, out var forbidden))
        {
            return forbidden;
        }

        var actorId = _currentUser.UserId ?? string.Empty;
        await _userManagement
            .ChangeUserRoleAsync(actorId, tenantId, userId, request.Role, cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>Set user inactive in this tenant.</summary>
    [HttpPost("{userId}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeactivateAsync(
        [FromRoute] string tenantId,
        [FromRoute] string userId,
        CancellationToken cancellationToken = default)
    {
        if (!EnsureTenantRouteMatchesContext(tenantId, out var forbidden))
        {
            return forbidden;
        }

        var actorId = _currentUser.UserId ?? string.Empty;
        await _userManagement
            .DeactivateUserAsync(actorId, tenantId, userId, cancellationToken)
            .ConfigureAwait(false);
        return Ok();
    }

    /// <summary>Set user active in this tenant.</summary>
    [HttpPost("{userId}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ActivateAsync(
        [FromRoute] string tenantId,
        [FromRoute] string userId,
        CancellationToken cancellationToken = default)
    {
        if (!EnsureTenantRouteMatchesContext(tenantId, out var forbidden))
        {
            return forbidden;
        }

        var actorId = _currentUser.UserId ?? string.Empty;
        await _userManagement
            .ActivateUserAsync(actorId, tenantId, userId, cancellationToken)
            .ConfigureAwait(false);
        return Ok();
    }

    private bool EnsureTenantRouteMatchesContext(string tenantId, out ActionResult forbidden)
    {
        if (!string.Equals(tenantId, _tenantContext.TenantId, StringComparison.Ordinal))
        {
            forbidden = Forbid();
            return false;
        }

        forbidden = default!;
        return true;
    }
}

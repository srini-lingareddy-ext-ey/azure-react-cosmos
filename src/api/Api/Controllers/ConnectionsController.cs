using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-45: connection CRUD with encrypted credentials, referential integrity, and test.</summary>
[ApiController]
[Route("api/v1/admin/connections")]
[Authorize]
[RequireTenantContext]
public sealed class ConnectionsController : ControllerBase
{
    private readonly IConnectionService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantContext _tenantContext;

    public ConnectionsController(
        IConnectionService service,
        ICurrentUserService currentUser,
        ICurrentTenantContext tenantContext)
    {
        _service = service;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ConnectionListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectionListResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListAsync(_tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ConnectionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectionResponse>> GetByIdAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ConnectionResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ConnectionResponse>> CreateAsync(
        [FromBody] CreateConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var created = await _service.CreateAsync(userId, _tenantContext.TenantId, request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(ConnectionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectionResponse>> UpdateAsync(
        [FromRoute] string id,
        [FromBody] UpdateConnectionRequest request,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var updated = await _service.UpdateAsync(userId, id, _tenantContext.TenantId, request, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        await _service.DeleteAsync(id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    [HttpPost("{id}/test")]
    [ProducesResponseType(typeof(ConnectionTestResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectionTestResponse>> TestAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.TestAsync(id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    private void RequireAdmin()
    {
        if (_tenantContext.Role != Todo.Api.Domain.Entities.UserRole.Admin
            && _tenantContext.Role != Todo.Api.Domain.Entities.UserRole.PlatformAdmin)
        {
            throw new UnauthorizedAccessException("This operation requires the Admin role.");
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-46: monitor CRUD with pause/activate.</summary>
[ApiController]
[Route("api/v1/admin/monitors")]
[Authorize]
[RequireTenantContext]
public sealed class MonitorsController : ControllerBase
{
    private readonly IMonitorService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantContext _tenantContext;

    public MonitorsController(
        IMonitorService service,
        ICurrentUserService currentUser,
        ICurrentTenantContext tenantContext)
    {
        _service = service;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(MonitorListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MonitorListResponse>> ListAsync(
        [FromQuery] string? status,
        [FromQuery] string? businessPlanId,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListAsync(_tenantContext.TenantId, status, businessPlanId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MonitorResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MonitorResponse>> GetByIdAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(MonitorResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<MonitorResponse>> CreateAsync(
        [FromBody] CreateMonitorRequest request,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var created = await _service.CreateAsync(userId, _tenantContext.TenantId, request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(MonitorResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MonitorResponse>> UpdateAsync(
        [FromRoute] string id,
        [FromBody] UpdateMonitorRequest request,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var updated = await _service.UpdateAsync(userId, id, _tenantContext.TenantId, request, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    [HttpPost("{id}/pause")]
    [ProducesResponseType(typeof(MonitorResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MonitorResponse>> PauseAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var result = await _service.PauseAsync(userId, id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(MonitorResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MonitorResponse>> ActivateAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var result = await _service.ActivateAsync(userId, id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
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

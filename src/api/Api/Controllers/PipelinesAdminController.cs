using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-43: pipeline registration CRUD with deactivation and monitor suspension.</summary>
[ApiController]
[Route("api/v1/admin/pipelines")]
[Authorize]
[RequireTenantContext]
public sealed class PipelinesAdminController : ControllerBase
{
    private readonly IPipelineRegistrationService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantContext _tenantContext;

    public PipelinesAdminController(
        IPipelineRegistrationService service,
        ICurrentUserService currentUser,
        ICurrentTenantContext tenantContext)
    {
        _service = service;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PipelineRegistrationListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PipelineRegistrationListResponse>> ListAsync(
        [FromQuery] string? businessPlanId,
        [FromQuery] string? medallionLayer,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListAsync(_tenantContext.TenantId, businessPlanId, medallionLayer, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PipelineRegistrationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PipelineRegistrationResponse>> GetByIdAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(PipelineRegistrationResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<PipelineRegistrationResponse>> CreateAsync(
        [FromBody] CreatePipelineRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var created = await _service.CreateAsync(userId, _tenantContext.TenantId, request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(PipelineRegistrationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PipelineRegistrationResponse>> UpdateAsync(
        [FromRoute] string id,
        [FromBody] UpdatePipelineRegistrationRequest request,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var updated = await _service.UpdateAsync(userId, id, _tenantContext.TenantId, request, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(typeof(PipelineDeactivateResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PipelineDeactivateResponse>> DeactivateAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var result = await _service.DeactivateAsync(userId, id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(PipelineRegistrationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PipelineRegistrationResponse>> ActivateAsync(
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

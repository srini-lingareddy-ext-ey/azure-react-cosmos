using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-42: business plan CRUD with activate/deactivate lifecycle.</summary>
[ApiController]
[Route("api/v1/admin/business-plans")]
[Authorize]
[RequireTenantContext]
public sealed class BusinessPlansController : ControllerBase
{
    private readonly IBusinessPlanService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantContext _tenantContext;

    public BusinessPlansController(
        IBusinessPlanService service,
        ICurrentUserService currentUser,
        ICurrentTenantContext tenantContext)
    {
        _service = service;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(BusinessPlanListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BusinessPlanListResponse>> ListAsync(
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListAsync(_tenantContext.TenantId, isActive, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(BusinessPlanResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BusinessPlanResponse>> GetByIdAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BusinessPlanResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<BusinessPlanResponse>> CreateAsync(
        [FromBody] CreateBusinessPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var created = await _service.CreateAsync(userId, _tenantContext.TenantId, request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(BusinessPlanResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BusinessPlanResponse>> UpdateAsync(
        [FromRoute] string id,
        [FromBody] UpdateBusinessPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var updated = await _service.UpdateAsync(userId, id, _tenantContext.TenantId, request, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(BusinessPlanResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BusinessPlanResponse>> ActivateAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var result = await _service.ActivateAsync(userId, id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(typeof(BusinessPlanResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<BusinessPlanResponse>> DeactivateAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var result = await _service.DeactivateAsync(userId, id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
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

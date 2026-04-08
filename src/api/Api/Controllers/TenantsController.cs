using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-9: tenant CRUD (authorization enforced in <see cref="ITenantService"/>).</summary>
[ApiController]
[Route("api/v1/tenants")]
[Authorize]
public sealed class TenantsController : ControllerBase
{
    private readonly ITenantService _tenantService;
    private readonly ICurrentUserService _currentUser;

    public TenantsController(ITenantService tenantService, ICurrentUserService currentUser)
    {
        _tenantService = tenantService;
        _currentUser = currentUser;
    }

    /// <summary>Paginated tenant list (PlatformAdmin only).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(TenantListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TenantListResponse>> ListAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? string.Empty;
        var result = await _tenantService
            .ListTenantsAsync(userId, page, pageSize, cancellationToken)
            .ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>Create tenant (PlatformAdmin only).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<TenantResponse>> CreateAsync(
        [FromBody] CreateTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? string.Empty;
        var created = await _tenantService
            .CreateTenantAsync(userId, request, cancellationToken)
            .ConfigureAwait(false);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
    }

    /// <summary>Get tenant by id (PlatformAdmin or Admin of that tenant).</summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TenantResponse>> GetByIdAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? string.Empty;
        var tenant = await _tenantService
            .GetTenantAsync(userId, id, cancellationToken)
            .ConfigureAwait(false);
        return Ok(tenant);
    }

    /// <summary>Merge partial config (PlatformAdmin or Admin of that tenant).</summary>
    [HttpPatch("{id}/config")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TenantResponse>> PatchConfigAsync(
        [FromRoute] string id,
        [FromBody] UpdateTenantConfigRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? string.Empty;
        var updated = await _tenantService
            .PatchTenantConfigAsync(userId, id, request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(updated);
    }

    /// <summary>Activate tenant (PlatformAdmin only).</summary>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TenantResponse>> ActivateAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? string.Empty;
        var updated = await _tenantService
            .ActivateTenantAsync(userId, id, cancellationToken)
            .ConfigureAwait(false);
        return Ok(updated);
    }

    /// <summary>Deactivate tenant (PlatformAdmin only).</summary>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<TenantResponse>> DeactivateAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? string.Empty;
        var updated = await _tenantService
            .DeactivateTenantAsync(userId, id, cancellationToken)
            .ConfigureAwait(false);
        return Ok(updated);
    }
}

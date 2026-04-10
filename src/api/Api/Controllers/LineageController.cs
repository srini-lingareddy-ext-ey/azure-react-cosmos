using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-44: pipeline lineage relationships with cycle detection.</summary>
[ApiController]
[Authorize]
[RequireTenantContext]
public sealed class LineageController : ControllerBase
{
    private readonly ILineageService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantContext _tenantContext;

    public LineageController(
        ILineageService service,
        ICurrentUserService currentUser,
        ICurrentTenantContext tenantContext)
    {
        _service = service;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    [HttpGet("api/v1/admin/pipelines/{pipelineId}/lineage")]
    [ProducesResponseType(typeof(PipelineLineageResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PipelineLineageResponse>> GetLineageAsync(
        [FromRoute] string pipelineId,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetLineageAsync(pipelineId, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("api/v1/admin/lineage")]
    [ProducesResponseType(typeof(LineageRelationshipResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<LineageRelationshipResponse>> CreateAsync(
        [FromBody] CreateLineageRequest request,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var created = await _service.CreateAsync(userId, _tenantContext.TenantId, request, cancellationToken).ConfigureAwait(false);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpDelete("api/v1/admin/lineage/{relationshipId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAsync(
        [FromRoute] string relationshipId,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        await _service.DeleteAsync(relationshipId, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return NoContent();
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

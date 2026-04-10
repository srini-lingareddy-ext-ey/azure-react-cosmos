using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-46: query template CRUD with propagation to monitors.</summary>
[ApiController]
[Route("api/v1/admin/query-templates")]
[Authorize]
[RequireTenantContext]
public sealed class QueryTemplatesController : ControllerBase
{
    private readonly IQueryTemplateService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantContext _tenantContext;

    public QueryTemplatesController(
        IQueryTemplateService service,
        ICurrentUserService currentUser,
        ICurrentTenantContext tenantContext)
    {
        _service = service;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(QueryTemplateListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<QueryTemplateListResponse>> ListAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListAsync(_tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(QueryTemplateResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<QueryTemplateResponse>> GetByIdAsync(
        [FromRoute] string id,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(QueryTemplateResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<QueryTemplateResponse>> CreateAsync(
        [FromBody] CreateQueryTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var created = await _service.CreateAsync(userId, _tenantContext.TenantId, request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(QueryTemplateResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<QueryTemplateResponse>> UpdateAsync(
        [FromRoute] string id,
        [FromBody] UpdateQueryTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var updated = await _service.UpdateAsync(userId, id, _tenantContext.TenantId, request, cancellationToken).ConfigureAwait(false);

        if (string.Equals(request.PropagationMode, "allExisting", StringComparison.OrdinalIgnoreCase))
            return Accepted(updated);

        return Ok(updated);
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

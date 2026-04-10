using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-67 / WO-70: Incident management endpoints.</summary>
[ApiController]
[Route("api/v1/incidents")]
[Authorize]
[RequireTenantContext]
public sealed class IncidentsController : ControllerBase
{
    private readonly IIncidentLifecycleService _lifecycleService;
    private readonly IIncidentQueryService _queryService;
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;

    public IncidentsController(IIncidentLifecycleService lifecycleService, IIncidentQueryService queryService, ICurrentTenantContext tenantContext, ICurrentUserService currentUser)
    { _lifecycleService = lifecycleService; _queryService = queryService; _tenantContext = tenantContext; _currentUser = currentUser; }

    [HttpGet]
    [ProducesResponseType(typeof(IncidentListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentListResponse>> ListAsync([FromQuery] string? state, [FromQuery] string? severity, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, [FromQuery] string? sort, [FromQuery] string? order, [FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken cancellationToken = default)
    { return Ok(await _queryService.GetIncidentsAsync(_tenantContext.TenantId, state, severity, from, to, sort, order, limit, offset, cancellationToken).ConfigureAwait(false)); }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(IncidentDetailDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<IncidentDetailDto>> GetByIdAsync([FromRoute] string id, CancellationToken cancellationToken = default)
    { return Ok(await _queryService.GetIncidentByIdAsync(id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false)); }

    [HttpPatch("{id}/state")]
    [Authorize(Roles = "Operator,Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(StateTransitionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<StateTransitionResponse>> TransitionStateAsync([FromRoute] string id, [FromBody] StateTransitionRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? string.Empty;
        var etag = Request.Headers.IfMatch.FirstOrDefault();
        return Ok(await _lifecycleService.TransitionStateAsync(id, _tenantContext.TenantId, userId, request, etag, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{id}/notes")]
    [Authorize(Roles = "Operator,Admin,PlatformAdmin")]
    [ProducesResponseType(typeof(AddNoteResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<AddNoteResponse>> AddNoteAsync([FromRoute] string id, [FromBody] AddNoteRequest request, CancellationToken cancellationToken = default)
    {
        var userId = _currentUser.UserId ?? string.Empty;
        var userName = userId;
        var result = await _lifecycleService.AddNoteAsync(id, _tenantContext.TenantId, userId, userName, request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetByIdAsync), new { id }, result);
    }

    [HttpPost("{id}/retry-ticket")]
    [ProducesResponseType(typeof(RetryTicketResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RetryTicketResponse>> RetryTicketAsync([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        return Ok(await _lifecycleService.RetryTicketAsync(id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false));
    }

    private void RequireAdmin()
    {
        if (_tenantContext.Role != Todo.Api.Domain.Entities.UserRole.Admin && _tenantContext.Role != Todo.Api.Domain.Entities.UserRole.PlatformAdmin)
            throw new UnauthorizedAccessException("This operation requires the Admin role.");
    }
}

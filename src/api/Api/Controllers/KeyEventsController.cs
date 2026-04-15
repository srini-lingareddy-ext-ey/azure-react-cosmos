using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Infrastructure;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-84: Key events timeline API.</summary>
[ApiController]
[Route("api/v1/events/key-events")]
[Authorize]
[RequireTenantContext]
public sealed class KeyEventsController : ControllerBase
{
    private readonly IKeyEventsService _service;
    private readonly ICurrentTenantContext _tenantContext;

    public KeyEventsController(IKeyEventsService service, ICurrentTenantContext tenantContext)
    { _service = service; _tenantContext = tenantContext; }

    [HttpGet]
    [ProducesResponseType(typeof(KeyEventsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<KeyEventsResponse>> GetKeyEventsAsync(
        [FromQuery] List<string>? classification,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] int limit = 50,
        [FromQuery] int offset = 0,
        CancellationToken ct = default)
    {
        return Ok(await _service.GetKeyEventsAsync(_tenantContext.TenantId, classification, from, to, limit, offset, ct).ConfigureAwait(false));
    }

    [HttpGet("{eventId}")]
    [ProducesResponseType(typeof(TimelineEntryDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorEnvelope), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetKeyEventByIdAsync(string eventId, CancellationToken ct = default)
    {
        var detail = await _service.GetKeyEventByIdAsync(eventId, _tenantContext.TenantId, ct).ConfigureAwait(false);
        if (detail is null) return NotFound();

        if (string.Equals(detail.Classification, nameof(EventClassification.Informational), StringComparison.OrdinalIgnoreCase)
            && _tenantContext.Role == UserRole.Viewer)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ApiErrorEnvelope(HttpContext.TraceIdentifier, "FORBIDDEN", "Viewers cannot access informational events."));
        }

        return Ok(detail);
    }
}
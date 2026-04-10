using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

[ApiController]
[Route("api/v1/events")]
[Authorize]
[RequireTenantContext]
public sealed class EventsController : ControllerBase
{
    private readonly IEventService _service;
    private readonly ICurrentTenantContext _tenantContext;

    public EventsController(IEventService service, ICurrentTenantContext tenantContext)
    {
        _service = service;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    public async Task<ActionResult<EventLogResponse>> GetEventsAsync(
        [FromQuery] string? classification, [FromQuery] string? severity,
        [FromQuery] string? sourceSystem, [FromQuery] string? businessPlan,
        [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to,
        [FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken ct = default) =>
        Ok(await _service.GetEventsAsync(_tenantContext.TenantId, classification, severity, sourceSystem, businessPlan, from, to, limit, offset, ct));

    [HttpGet("{eventId}")]
    public async Task<IActionResult> GetEventByIdAsync(string eventId, CancellationToken ct = default)
    {
        var detail = await _service.GetEventByIdAsync(eventId, _tenantContext.TenantId, ct);
        return detail is null ? NotFound() : Ok(detail);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-71: Notification configuration management (Admin only).</summary>
[ApiController]
[Route("api/v1/notifications")]
[Authorize(Roles = "Admin,PlatformAdmin")]
[RequireTenantContext]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationsConfigService _service;
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(INotificationsConfigService service, ICurrentTenantContext tenantContext, ICurrentUserService currentUser)
    { _service = service; _tenantContext = tenantContext; _currentUser = currentUser; }

    [HttpGet("channels")] public async Task<ActionResult<List<NotificationChannelDto>>> GetChannelsAsync(CancellationToken ct = default) => Ok(await _service.GetChannelsAsync(_tenantContext.TenantId, ct).ConfigureAwait(false));
    [HttpGet("channels/{id}")] public async Task<ActionResult<NotificationChannelDto>> GetChannelByIdAsync(string id, CancellationToken ct = default) => Ok(await _service.GetChannelByIdAsync(id, _tenantContext.TenantId, ct).ConfigureAwait(false));
    [HttpPost("channels")] public async Task<ActionResult<NotificationChannelDto>> CreateChannelAsync([FromBody] CreateNotificationChannelRequest request, CancellationToken ct = default) { var r = await _service.CreateChannelAsync(_tenantContext.TenantId, _currentUser.UserId ?? string.Empty, request, ct).ConfigureAwait(false); return CreatedAtAction(nameof(GetChannelByIdAsync), new { id = r.Id }, r); }
    [HttpPatch("channels/{id}")] public async Task<ActionResult<NotificationChannelDto>> UpdateChannelAsync(string id, [FromBody] UpdateNotificationChannelRequest request, CancellationToken ct = default) => Ok(await _service.UpdateChannelAsync(id, _tenantContext.TenantId, _currentUser.UserId ?? string.Empty, request, ct).ConfigureAwait(false));
    [HttpDelete("channels/{id}")] public async Task<IActionResult> DeleteChannelAsync(string id, CancellationToken ct = default) { await _service.DeleteChannelAsync(id, _tenantContext.TenantId, ct).ConfigureAwait(false); return NoContent(); }

    [HttpGet("routing-rules")] public async Task<ActionResult<List<NotificationRoutingRuleDto>>> GetRoutingRulesAsync(CancellationToken ct = default) => Ok(await _service.GetRoutingRulesAsync(_tenantContext.TenantId, ct).ConfigureAwait(false));
    [HttpPost("routing-rules")] public async Task<ActionResult<NotificationRoutingRuleDto>> CreateRoutingRuleAsync([FromBody] CreateNotificationRoutingRuleRequest request, CancellationToken ct = default) { var r = await _service.CreateRoutingRuleAsync(_tenantContext.TenantId, _currentUser.UserId ?? string.Empty, request, ct).ConfigureAwait(false); return StatusCode(StatusCodes.Status201Created, r); }
    [HttpPatch("routing-rules/{id}")] public async Task<ActionResult<NotificationRoutingRuleDto>> UpdateRoutingRuleAsync(string id, [FromBody] UpdateNotificationRoutingRuleRequest request, CancellationToken ct = default) => Ok(await _service.UpdateRoutingRuleAsync(id, _tenantContext.TenantId, _currentUser.UserId ?? string.Empty, request, ct).ConfigureAwait(false));
    [HttpDelete("routing-rules/{id}")] public async Task<IActionResult> DeleteRoutingRuleAsync(string id, CancellationToken ct = default) { await _service.DeleteRoutingRuleAsync(id, _tenantContext.TenantId, ct).ConfigureAwait(false); return NoContent(); }

    [HttpGet("maintenance-windows")] public async Task<ActionResult<List<MaintenanceWindowDto>>> GetMaintenanceWindowsAsync(CancellationToken ct = default) => Ok(await _service.GetMaintenanceWindowsAsync(_tenantContext.TenantId, ct).ConfigureAwait(false));
    [HttpPost("maintenance-windows")] public async Task<ActionResult<MaintenanceWindowDto>> CreateMaintenanceWindowAsync([FromBody] CreateMaintenanceWindowRequest request, CancellationToken ct = default) { var r = await _service.CreateMaintenanceWindowAsync(_tenantContext.TenantId, _currentUser.UserId ?? string.Empty, request, ct).ConfigureAwait(false); return StatusCode(StatusCodes.Status201Created, r); }
    [HttpDelete("maintenance-windows/{id}")] public async Task<IActionResult> DeleteMaintenanceWindowAsync(string id, CancellationToken ct = default) { await _service.DeleteMaintenanceWindowAsync(id, _tenantContext.TenantId, ct).ConfigureAwait(false); return NoContent(); }

    [HttpGet("delivery-logs")] public async Task<ActionResult<List<NotificationDeliveryLogDto>>> GetDeliveryLogsAsync([FromQuery] string? eventId, [FromQuery] string? status, [FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, [FromQuery] int limit = 50, CancellationToken ct = default) => Ok(await _service.GetDeliveryLogsAsync(_tenantContext.TenantId, eventId, status, from, to, limit, ct).ConfigureAwait(false));

    [HttpGet("servicenow-config")] public async Task<ActionResult<ServiceNowConfigDto?>> GetServiceNowConfigAsync(CancellationToken ct = default) => Ok(await _service.GetServiceNowConfigAsync(_tenantContext.TenantId, ct).ConfigureAwait(false));
    [HttpPut("servicenow-config")] public async Task<ActionResult<ServiceNowConfigDto>> UpsertServiceNowConfigAsync([FromBody] UpsertServiceNowConfigRequest request, CancellationToken ct = default) => Ok(await _service.UpsertServiceNowConfigAsync(_tenantContext.TenantId, _currentUser.UserId ?? string.Empty, request, ct).ConfigureAwait(false));
}

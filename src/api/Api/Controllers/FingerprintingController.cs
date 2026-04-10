using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

[ApiController]
[Route("api/v1/compliance/fingerprints")]
[Authorize]
[RequireTenantContext]
public sealed class FingerprintingController : ControllerBase
{
    private readonly IFingerprintingService _service;
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ICurrentUserService _userService;

    public FingerprintingController(IFingerprintingService service, ICurrentTenantContext tenantContext, ICurrentUserService userService)
    {
        _service = service;
        _tenantContext = tenantContext;
        _userService = userService;
    }

    [HttpGet("artifacts")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<List<MonitoredArtifactView>>> GetArtifactsAsync(CancellationToken ct = default) =>
        Ok(await _service.GetArtifactsAsync(_tenantContext.TenantId, ct));

    [HttpPost("artifacts")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<MonitoredArtifactView>> RegisterArtifactAsync([FromBody] RegisterArtifactRequest request, CancellationToken ct = default) =>
        StatusCode(201, await _service.RegisterArtifactAsync(_tenantContext.TenantId, _userService.UserId ?? string.Empty, request, ct));

    [HttpPost("artifacts/{artifactId}/scan")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> TriggerScanAsync(string artifactId, CancellationToken ct = default)
    {
        try
        {
            await _service.TriggerScanAsync(artifactId, _tenantContext.TenantId, ct);
            return Accepted();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpPost("artifacts/{artifactId}/reset-baseline")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> ResetBaselineAsync(string artifactId, [FromBody] ResetBaselineRequest request, CancellationToken ct = default)
    {
        try
        {
            await _service.ResetBaselineAsync(artifactId, _tenantContext.TenantId, _userService.UserId ?? string.Empty, request.Justification, ct);
            return Ok();
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }

    [HttpGet("audit-trail")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<List<FingerprintAuditEntryView>>> GetAuditTrailAsync(
        [FromQuery] string? changeClassification, [FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken ct = default) =>
        Ok(await _service.GetAuditTrailAsync(_tenantContext.TenantId, changeClassification, limit, offset, ct));

    [HttpGet("windows")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<List<ApprovedWindowView>>> GetWindowsAsync(CancellationToken ct = default) =>
        Ok(await _service.GetWindowsAsync(_tenantContext.TenantId, ct));

    [HttpPost("windows")]
    [Authorize(Policy = "Admin")]
    public async Task<ActionResult<ApprovedWindowView>> CreateWindowAsync([FromBody] CreateApprovedWindowRequest request, CancellationToken ct = default) =>
        StatusCode(201, await _service.CreateWindowAsync(_tenantContext.TenantId, _userService.UserId ?? string.Empty, request, ct));

    [HttpDelete("windows/{windowId}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> DeleteWindowAsync(string windowId, CancellationToken ct = default)
    {
        await _service.DeleteWindowAsync(windowId, _tenantContext.TenantId, ct);
        return NoContent();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

[ApiController]
[Route("api/v1/sla")]
[Authorize]
[RequireTenantContext]
public sealed class SLAController : ControllerBase
{
    private readonly ISLAService _service;
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;

    public SLAController(ISLAService service, ICurrentTenantContext tenantContext, ICurrentUserService currentUser)
    {
        _service = service;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    [HttpGet("status")]
    public async Task<ActionResult<List<SLAStatusDto>>> GetStatusAsync([FromQuery] string? status, CancellationToken ct = default) =>
        Ok(await _service.GetStatusAsync(_tenantContext.TenantId, status, ct));

    [HttpGet("compliance")]
    public async Task<ActionResult<SLAComplianceResponse>> GetComplianceAsync([FromQuery] string? timeRange, CancellationToken ct = default) =>
        Ok(await _service.GetComplianceAsync(_tenantContext.TenantId, timeRange, ct));

    [HttpGet("history/{pipelineId}")]
    public async Task<ActionResult<List<SLABreachHistoryDto>>> GetHistoryAsync(string pipelineId, [FromQuery] int limit = 30, CancellationToken ct = default) =>
        Ok(await _service.GetHistoryAsync(_tenantContext.TenantId, pipelineId, limit, ct));

    [HttpPost("config/{pipelineId}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> UpsertConfigAsync(string pipelineId, [FromBody] SLAConfigRequest request, CancellationToken ct = default)
    {
        var isNew = await _service.UpsertConfigAsync(_tenantContext.TenantId, pipelineId, request, _currentUser.UserId ?? string.Empty, ct);
        return isNew ? StatusCode(201) : Ok();
    }
}

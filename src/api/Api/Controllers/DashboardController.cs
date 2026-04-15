using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-82: Health Bar and Dashboard Config API.</summary>
[ApiController]
[Authorize]
[RequireTenantContext]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    private readonly ICurrentTenantContext _tenantContext;

    public DashboardController(IDashboardService service, ICurrentTenantContext tenantContext)
    { _service = service; _tenantContext = tenantContext; }

    [HttpGet("api/v1/health-score/current")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetHealthScoreAsync(CancellationToken ct = default)
    {
        var score = await _service.GetHealthScoreAsync(_tenantContext.TenantId, ct).ConfigureAwait(false);
        return score is null ? NotFound() : Ok(score);
    }

    [HttpGet("api/v1/dashboard/config")]
    [ProducesResponseType(typeof(DashboardConfigResponse), 200)]
    public async Task<ActionResult<DashboardConfigResponse>> GetDashboardConfigAsync(CancellationToken ct = default)
    {
        var config = await _service.GetDashboardConfigAsync(_tenantContext.TenantId, _tenantContext.Role.ToString(), ct).ConfigureAwait(false);
        return Ok(config);
    }
}
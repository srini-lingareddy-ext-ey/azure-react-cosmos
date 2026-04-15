using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-83: Dashboard KPIs and Charts API.</summary>
[ApiController]
[Authorize]
[RequireTenantContext]
public sealed class DashboardChartsController : ControllerBase
{
    private readonly IDashboardChartsService _service;
    private readonly ICurrentTenantContext _tenantContext;

    public DashboardChartsController(IDashboardChartsService service, ICurrentTenantContext tenantContext)
    { _service = service; _tenantContext = tenantContext; }

    [HttpGet("api/v1/dashboard/kpis")]
    [ProducesResponseType(typeof(DashboardKpisResponse), 200)]
    public async Task<ActionResult<DashboardKpisResponse>> GetKpisAsync(CancellationToken ct = default)
    {
        return Ok(await _service.GetKpisAsync(_tenantContext.TenantId, ct).ConfigureAwait(false));
    }

    [HttpGet("api/v1/dashboard/charts")]
    [ProducesResponseType(typeof(DashboardChartsResponse), 200)]
    public async Task<ActionResult<DashboardChartsResponse>> GetChartsAsync(
        [FromQuery] string timeRange = "last7d",
        CancellationToken ct = default)
    {
        return Ok(await _service.GetChartsAsync(_tenantContext.TenantId, timeRange, ct).ConfigureAwait(false));
    }
}
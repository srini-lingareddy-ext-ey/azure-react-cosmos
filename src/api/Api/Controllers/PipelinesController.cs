using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

[ApiController]
[Route("api/v1/pipelines")]
[Authorize]
[RequireTenantContext]
public sealed class PipelinesController : ControllerBase
{
    private readonly IPipelineMonitoringService _service;
    private readonly ICurrentTenantContext _tenantContext;

    public PipelinesController(IPipelineMonitoringService service, ICurrentTenantContext tenantContext)
    {
        _service = service;
        _tenantContext = tenantContext;
    }

    [HttpGet("status")]
    public async Task<ActionResult<PipelineStatusListResponse>> GetStatusAsync([FromQuery] string? status, [FromQuery] string? businessPlan, [FromQuery] int limit = 50, [FromQuery] int offset = 0, CancellationToken ct = default) =>
        Ok(await _service.GetStatusAsync(_tenantContext.TenantId, status, businessPlan, limit, offset, ct));

    [HttpGet("executions/{executionId}/hops/{layer}")]
    public async Task<IActionResult> GetHopDetailAsync(string executionId, string layer, CancellationToken ct = default)
    {
        var result = await _service.GetHopDetailAsync(_tenantContext.TenantId, executionId, layer, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("~/api/v1/memsql/interfaces")]
    public async Task<ActionResult<List<MemSQLInterfaceDto>>> GetMemSQLInterfacesAsync([FromQuery] string? status, CancellationToken ct = default) =>
        Ok(await _service.GetMemSQLInterfacesAsync(_tenantContext.TenantId, status, ct));
}

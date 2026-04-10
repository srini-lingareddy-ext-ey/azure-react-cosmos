using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

[ApiController]
[Route("api/v1/infrastructure")]
[Authorize]
[RequireTenantContext]
public sealed class InfrastructureController : ControllerBase
{
    private readonly IInfrastructureMonitoringService _service;
    private readonly ICurrentTenantContext _tenantContext;

    public InfrastructureController(IInfrastructureMonitoringService service, ICurrentTenantContext tenantContext)
    {
        _service = service;
        _tenantContext = tenantContext;
    }

    [HttpGet("status")]
    public async Task<ActionResult<InfrastructureStatusResponse>> GetStatusAsync([FromQuery] string? status, CancellationToken ct = default) =>
        Ok(await _service.GetStatusAsync(_tenantContext.TenantId, status, ct));

    [HttpGet("components/{componentId}/nodes")]
    public async Task<IActionResult> GetComponentNodesAsync(string componentId, CancellationToken ct = default)
    {
        var result = await _service.GetComponentNodesAsync(_tenantContext.TenantId, componentId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("nodes/{nodeId}/metrics")]
    public async Task<ActionResult<List<NodeMetricDto>>> GetNodeMetricsAsync(string nodeId, CancellationToken ct = default) =>
        Ok(await _service.GetNodeMetricsAsync(_tenantContext.TenantId, nodeId, ct));

    [HttpGet("products/{productId}/availability")]
    public async Task<IActionResult> GetProductAvailabilityAsync(string productId, [FromQuery] int trendDays = 30, CancellationToken ct = default)
    {
        var result = await _service.GetProductAvailabilityAsync(_tenantContext.TenantId, productId, trendDays, ct);
        return result is null ? NotFound() : Ok(result);
    }
}

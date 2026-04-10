using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

[ApiController]
[Authorize]
[RequireTenantContext]
public sealed class JobsController : ControllerBase
{
    private readonly IJobMonitoringService _service;
    private readonly ICurrentTenantContext _tenantContext;

    public JobsController(IJobMonitoringService service, ICurrentTenantContext tenantContext)
    {
        _service = service;
        _tenantContext = tenantContext;
    }

    [HttpGet("api/v1/pipelines/executions/{executionId}/jobs")]
    public async Task<ActionResult<List<JobRunDto>>> GetJobsByExecutionAsync(string executionId, CancellationToken ct = default) =>
        Ok(await _service.GetJobsByExecutionAsync(_tenantContext.TenantId, executionId, ct));

    [HttpGet("api/v1/pipelines/executions/{executionId}/jobs/{jobName}")]
    public async Task<IActionResult> GetJobDetailAsync(string executionId, string jobName, CancellationToken ct = default)
    {
        var result = await _service.GetJobDetailAsync(_tenantContext.TenantId, executionId, jobName, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("api/v1/pipelines/{pipelineId}/jobs/{jobName}/history")]
    public async Task<IActionResult> GetJobHistoryAsync(string pipelineId, string jobName, [FromQuery] int days = 30, CancellationToken ct = default)
    {
        var result = await _service.GetJobHistoryAsync(_tenantContext.TenantId, pipelineId, jobName, days, ct);
        return result is null ? NotFound() : Ok(result);
    }
}

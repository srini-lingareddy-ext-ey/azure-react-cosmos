using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

[ApiController]
[Route("api/v1/data-quality")]
[Authorize]
[RequireTenantContext]
public sealed class DataQualityController : ControllerBase
{
    private readonly IDataQualityService _service;
    private readonly ICurrentTenantContext _tenantContext;
    private readonly ICurrentUserService _currentUser;

    public DataQualityController(IDataQualityService service, ICurrentTenantContext tenantContext, ICurrentUserService currentUser)
    {
        _service = service;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
    }

    [HttpGet("status")]
    public async Task<ActionResult<List<DataQualityStatusDto>>> GetStatusAsync([FromQuery] string? qualityStatus, CancellationToken ct = default) =>
        Ok(await _service.GetStatusAsync(_tenantContext.TenantId, qualityStatus, ct));

    [HttpGet("{pipelineId}/trend")]
    public async Task<ActionResult<List<DataQualityTrendPointDto>>> GetTrendAsync(string pipelineId, [FromQuery] int days = 7, CancellationToken ct = default) =>
        Ok(await _service.GetTrendAsync(_tenantContext.TenantId, pipelineId, days, ct));

    [HttpGet("{pipelineId}/scores/{scoreId}/checks")]
    public async Task<IActionResult> GetChecksAsync(string pipelineId, string scoreId, CancellationToken ct = default)
    {
        var result = await _service.GetChecksAsync(_tenantContext.TenantId, pipelineId, scoreId, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("config/{pipelineId}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> UpsertConfigAsync(string pipelineId, [FromBody] DataQualityThresholdRequest request, CancellationToken ct = default)
    {
        var isNew = await _service.UpsertConfigAsync(_tenantContext.TenantId, pipelineId, request, _currentUser.UserId ?? string.Empty, ct);
        return isNew ? StatusCode(201) : Ok();
    }
}

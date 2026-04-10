using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Api.Controllers;

/// <summary>WO-47: connector CRUD, enable/disable, test, logs, and type catalog.</summary>
[ApiController]
[Route("api/v1/connectors")]
[Authorize]
[RequireTenantContext]
public sealed class ConnectorsController : ControllerBase
{
    private readonly IConnectorService _service;
    private readonly ICurrentUserService _currentUser;
    private readonly ICurrentTenantContext _tenantContext;

    public ConnectorsController(
        IConnectorService service,
        ICurrentUserService currentUser,
        ICurrentTenantContext tenantContext)
    {
        _service = service;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    [HttpGet("catalog")]
    [ProducesResponseType(typeof(IReadOnlyList<ConnectorTypeCatalogEntryDto>), StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<ConnectorTypeCatalogEntryDto>> GetCatalog()
    {
        return Ok(_service.GetCatalog());
    }

    [HttpGet]
    [ProducesResponseType(typeof(ConnectorListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectorListResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var result = await _service.ListAsync(_tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ConnectorResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectorResponse>> GetByIdAsync([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetByIdAsync(id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ConnectorResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<ConnectorResponse>> CreateAsync(
        [FromBody] CreateConnectorRequest request, CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var created = await _service.CreateAsync(userId, _tenantContext.TenantId, request, cancellationToken).ConfigureAwait(false);
        return CreatedAtAction(nameof(GetByIdAsync), new { id = created.Id }, created);
    }

    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(ConnectorResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectorResponse>> UpdateAsync(
        [FromRoute] string id, [FromBody] UpdateConnectorRequest request, CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var updated = await _service.UpdateAsync(userId, id, _tenantContext.TenantId, request, cancellationToken).ConfigureAwait(false);
        return Ok(updated);
    }

    [HttpPost("{id}/enable")]
    [ProducesResponseType(typeof(ConnectorResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectorResponse>> EnableAsync([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var result = await _service.EnableAsync(userId, id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("{id}/disable")]
    [ProducesResponseType(typeof(ConnectorResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectorResponse>> DisableAsync([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        RequireAdmin();
        var userId = _currentUser.UserId ?? string.Empty;
        var result = await _service.DisableAsync(userId, id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpPost("{id}/test")]
    [ProducesResponseType(typeof(ConnectorTestResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectorTestResponse>> TestAsync([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var result = await _service.TestAsync(id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    [HttpGet("{id}/logs")]
    [ProducesResponseType(typeof(ConnectorLogResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ConnectorLogResponse>> GetLogsAsync([FromRoute] string id, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetLogsAsync(id, _tenantContext.TenantId, cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    private void RequireAdmin()
    {
        if (_tenantContext.Role != Todo.Api.Domain.Entities.UserRole.Admin
            && _tenantContext.Role != Todo.Api.Domain.Entities.UserRole.PlatformAdmin)
        {
            throw new UnauthorizedAccessException("This operation requires the Admin role.");
        }
    }
}

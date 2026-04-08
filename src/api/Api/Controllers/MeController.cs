using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Api.Authorization;
using Todo.Api.Application.Services;

namespace Todo.Api.Api.Controllers;

/// <summary>Authenticated caller context within an application tenant (WO-6).</summary>
[ApiController]
[Route("api/v1/me")]
[Authorize]
[RequireTenantContext]
public sealed class MeController : ControllerBase
{
    /// <summary>Returns the resolved tenant id and role for the current request (requires <c>X-Tenant-Id</c>).</summary>
    [HttpGet("tenant-context")]
    public ActionResult<TenantContextResponse> GetTenantContext([FromServices] ICurrentTenantContext tenantContext)
    {
        if (!tenantContext.IsSet)
            return StatusCode(StatusCodes.Status500InternalServerError);

        return Ok(new TenantContextResponse(
            tenantContext.TenantId,
            tenantContext.Role.ToString(),
            tenantContext.AssignmentStatus.ToString()));
    }
}

public sealed record TenantContextResponse(string TenantId, string Role, string Status);

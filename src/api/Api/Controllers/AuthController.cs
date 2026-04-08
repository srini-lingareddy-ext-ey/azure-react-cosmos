using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Todo.Api.Application.Services;

namespace Todo.Api.Api.Controllers;

/// <summary>
/// Authentication identity for the caller (WO-7 bypass path; expanded in WO-8).
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Authorize]
public sealed class AuthController : ControllerBase
{
    /// <summary>Returns the authenticated user id. Does not require <c>X-Tenant-Id</c>.</summary>
    [HttpGet("me")]
    public ActionResult<AuthMeResponse> GetMe([FromServices] ICurrentUserService users)
    {
        var id = users.UserId ?? string.Empty;
        return Ok(new AuthMeResponse(
            Id: id,
            DisplayName: null,
            Email: null,
            Tenants: Array.Empty<AuthMeTenantRefDto>()));
    }
}

public sealed record AuthMeResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("displayName")] string? DisplayName,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("tenants")] IReadOnlyList<AuthMeTenantRefDto> Tenants);

public sealed record AuthMeTenantRefDto(
    [property: JsonPropertyName("tenantId")] string TenantId,
    [property: JsonPropertyName("name")] string? Name);

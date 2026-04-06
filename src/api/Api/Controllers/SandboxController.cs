using Microsoft.AspNetCore.Mvc;
using Todo.Api.Application.Transport;
using Todo.Api.Infrastructure.RateLimiting;

namespace Todo.Api.Api.Controllers;

[ApiController]
[Route("api/v1/sandbox")]
public sealed class SandboxController : ControllerBase
{
    [HttpPost("validate")]
    [RateLimitPolicy(RateLimitTier.Write)]
    public IActionResult Validate([FromBody] SandboxValidateRequest request)
    {
        return Ok(new { message = request.Message });
    }
}

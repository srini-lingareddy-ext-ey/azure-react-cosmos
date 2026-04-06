namespace Todo.Api.Application.Transport;

/// <summary>
/// Sandbox POST body for FluentValidation pipeline and rate-limit integration tests (REQ-FOUNDATION-008 / 005).
/// </summary>
public sealed class SandboxValidateRequest
{
    public string Message { get; set; } = string.Empty;
}

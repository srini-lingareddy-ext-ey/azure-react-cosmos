namespace Todo.Api.Infrastructure;

/// <summary>
/// Centralized error codes for API responses (AC-FOUNDATION-007.4). Uppercase snake_case, descriptive.
/// </summary>
public static class ErrorCodes
{
    // Authentication and authorization (also used by JWT middleware)
    public const string Unauthorized = "UNAUTHORIZED";
    public const string Forbidden = "FORBIDDEN";

    /// <summary>Missing, invalid, or unauthorized tenant context (WO-7).</summary>
    public const string TenantAccessDenied = "TENANT_ACCESS_DENIED";

    /// <summary>Resolved tenant exists but is not active (WO-7).</summary>
    public const string TenantInactive = "TENANT_INACTIVE";

    // Client errors
    public const string BadRequest = "BAD_REQUEST";
    /// <summary>Transport/input validation failed (FluentValidation, HTTP 400).</summary>
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";
    public const string PreconditionFailed = "PRECONDITION_FAILED";
    public const string UnprocessableEntity = "UNPROCESSABLE_ENTITY";

    // Concurrency (domain)
    public const string ConcurrencyConflict = "CONCURRENCY_CONFLICT";

    // Server and dependency
    public const string InternalServerError = "INTERNAL_SERVER_ERROR";
    public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
}

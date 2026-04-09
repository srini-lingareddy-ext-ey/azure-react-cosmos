using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Todo.Api.Domain.Exceptions;
using Todo.Api.Infrastructure;

namespace Todo.Api.Infrastructure.Middleware;

/// <summary>
/// Global exception middleware. Catches unhandled exceptions and returns a standardized error envelope
/// with traceId, errorCode, and message (AC-FOUNDATION-007.1, 007.2, 007.5–007.7). Logs exceptions
/// with structured data including correlation ID for Application Insights (AC-FOUNDATION-007.3, 007.6).
/// If the response has already started (e.g. failure while writing the body), does not write an envelope;
/// logs a warning and rethrows to avoid corrupting the response stream.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
    };

    private const string GenericErrorMessage = "An unexpected error occurred. Use the trace ID for support.";

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex).ConfigureAwait(false);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                exception,
                "Exception after response started; cannot write error envelope. TraceId={TraceId}",
                context.TraceIdentifier);
            throw exception;
        }

        var traceId = context.TraceIdentifier;
        var (statusCode, errorCode, message) = MapException(exception);

        _logger.LogError(
            exception,
            "Unhandled exception: {ExceptionType}, TraceId={TraceId}, ErrorCode={ErrorCode}, Message={Message}",
            exception.GetType().Name,
            traceId,
            errorCode,
            exception.Message);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";

        var envelope = new ApiErrorEnvelope(traceId, errorCode, message);
        var json = JsonSerializer.Serialize(envelope, JsonOptions);
        await context.Response.WriteAsync(json).ConfigureAwait(false);
    }

    /// <summary>
    /// Maps exception types to HTTP status code, error code, and user-friendly message.
    /// Domain exceptions are handled separately from infrastructure exceptions (AC-FOUNDATION-007.5).
    /// </summary>
    private static (HttpStatusCode statusCode, string errorCode, string message) MapException(Exception exception)
    {
        return exception switch
        {
            ConcurrencyConflictException ex => (HttpStatusCode.PreconditionFailed, ErrorCodes.ConcurrencyConflict, ex.Message),

            TenantNameConflictException ex => (HttpStatusCode.Conflict, ErrorCodes.Conflict, ex.Message),

            ActiveUserAssignmentConflictException ex => (HttpStatusCode.Conflict, ErrorCodes.Conflict, ex.Message),

            PendingInvitationConflictException ex => (HttpStatusCode.Conflict, ErrorCodes.Conflict, ex.Message),

            UnauthorizedAccessException ex => (HttpStatusCode.Forbidden, ErrorCodes.Forbidden, string.IsNullOrEmpty(ex.Message) ? "Access denied." : ex.Message),

            KeyNotFoundException ex => (HttpStatusCode.NotFound, ErrorCodes.NotFound, ex.Message),
            ArgumentNullException ex => (HttpStatusCode.BadRequest, ErrorCodes.BadRequest, ex.Message ?? "A required value was missing."),
            ArgumentException ex => (HttpStatusCode.BadRequest, ErrorCodes.BadRequest, string.IsNullOrEmpty(ex.Message) ? "Invalid request." : ex.Message),

            InvalidOperationException ex => (HttpStatusCode.UnprocessableEntity, ErrorCodes.UnprocessableEntity, ex.Message),

            TimeoutException => (HttpStatusCode.ServiceUnavailable, ErrorCodes.ServiceUnavailable, "The operation timed out. Please try again."),
            HttpRequestException => (HttpStatusCode.ServiceUnavailable, ErrorCodes.ServiceUnavailable, "A dependency is temporarily unavailable. Please try again."),
            // Intentional: both client-aborted requests and server-side timeout/cancellation map to 503.
            // Distinguishing (e.g. client disconnect → 499) can be added later if needed.
            OperationCanceledException => (HttpStatusCode.ServiceUnavailable, ErrorCodes.ServiceUnavailable, "The request was cancelled or timed out."),

            _ => (HttpStatusCode.InternalServerError, ErrorCodes.InternalServerError, GenericErrorMessage)
        };
    }
}

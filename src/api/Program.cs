// =============================================================================
// Clean Architecture Layer Structure
// =============================================================================
// This solution follows Clean Architecture. Dependencies flow inward:
//   API (Controllers, DTOs) → Application (Services, Interfaces) → Domain (Entities, Interfaces)
//   Infrastructure (Repositories, Configuration) → Domain
//
// Layer directories:
//   Api/         — HTTP entry point; controllers and DTOs (see Api/README.md)
//   Application/ — Use cases, services, application interfaces (see Application/README.md)
//   Domain/      — Entities and domain interfaces; no outward dependencies (see Domain/README.md)
//   Infrastructure/ — Repositories, configuration, external services (see Infrastructure/README.md)
//
// This file is the Composition Root: dependencies are wired here, middleware is
// configured, and the application is bootstrapped. Register Infrastructure and
// Application services below as those layers are implemented.
// =============================================================================

using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Todo.Api.Infrastructure.Configuration;
using Todo.Api.Infrastructure.Cors;
using Todo.Api.Infrastructure.HealthChecks;
using Todo.Api.Infrastructure.Middleware;
using Todo.Api.Infrastructure.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// AC-FOUNDATION-009.3: Local development uses dotnet user-secrets for sensitive values.
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets(typeof(Program).Assembly, optional: true);
}

// AC-FOUNDATION-009.1, 009.2: Key Vault only in non-Development (Azure). In Development we use user-secrets only, even if AZURE_KEY_VAULT_ENDPOINT is set.
if (!builder.Environment.IsDevelopment())
{
    var keyVaultEndpoint = Environment.GetEnvironmentVariable("AZURE_KEY_VAULT_ENDPOINT");
    if (!string.IsNullOrEmpty(keyVaultEndpoint) && Uri.TryCreate(keyVaultEndpoint, UriKind.Absolute, out var keyVaultUri))
    {
        builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
    }
}

// Application services: register domain use cases here as they are introduced.

// AC-FOUNDATION-003: JWT bearer authentication with Microsoft Entra ID; 401/403 standardized responses; Admin role policy
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Todo.Api.Application.Services.ICurrentUserService, Todo.Api.Infrastructure.Identity.CurrentUserService>();
// WO-6: X-Tenant-Id + assignment validation for endpoints marked [RequireTenantContext]
builder.Services.AddTenantContext();

// AC-FOUNDATION-010.1–010.4, 010.7: IDistributedCache (in-memory in Development, Redis in staging/prod) with 2s timeout and graceful degradation
builder.Services.AddDistributedCache(builder.Configuration, builder.Environment);

// AC-FOUNDATION-005: rate limiting — single path: DistributedRateLimitingMiddleware (Redis or IDistributedCache)
builder.Services.AddDistributedRateLimiting(builder.Configuration);

// AC-FOUNDATION-011: HTTP resilience (retry, circuit breaker, timeout) for outbound calls. Config: Resilience:Http.
builder.Services.AddHttpResilience(builder.Configuration);

// Cosmos DB client (session consistency, RU monitoring) and repository pattern — see Domain/Repositories/IRepository.cs
builder.Services.AddCosmosDbClient(builder.Configuration);
// WO-4 / WO-5: Cosmos repositories when configured; AC-FOUNDATION-002.7 end-to-end registration
if (!string.IsNullOrEmpty(builder.Configuration["AZURE_COSMOS_ENDPOINT"]))
{
    var db = builder.Configuration["AZURE_COSMOS_DATABASE_NAME"] ?? "App";
    builder.Services.AddAppCosmosRepositories(db);
    builder.Services.AddScoped<Todo.Api.Application.Services.IAuthService, Todo.Api.Application.Services.AuthService>();
}
else
{
    // WO-8: GET /api/v1/auth/me without Cosmos returns claims-only profile for local development.
    builder.Services.AddScoped<Todo.Api.Application.Services.IAuthService, Todo.Api.Infrastructure.Identity.ClaimsOnlyAuthService>();
}
// AC-FOUNDATION-008: FluentValidation — DI, auto-validation before controllers, 400 envelope with field errors
builder.Services.AddFluentValidationPipeline();

// AC-FOUNDATION-006: CORS — default policy from config (resolved when first needed)
builder.Services.AddConfiguredCors();
// AC-FOUNDATION-004: liveness/readiness with optional Cosmos + Redis dependency checks
builder.Services.AddApplicationHealthChecks(builder.Configuration, builder.Environment);
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

CorsPolicySettings.Resolve(app.Configuration, app.Environment)
    .LogStartupIssues(app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Cors"));

// AC-FOUNDATION-007: Global exception handling — standardized error envelope (traceId, errorCode, message)
app.UseGlobalExceptionHandling();

// Required so endpoint metadata (e.g. [RequireTenantContext]) is available to middleware (WO-6).
app.UseRouting();

app.UseCors();

// AC-FOUNDATION-003: Authentication and authorization middleware (JWT validation, role checks)
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TenantContextMiddleware>();

app.UseMiddleware<DistributedRateLimitingMiddleware>();

// Swagger UI
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("./openapi.yaml", "v1");
    options.RoutePrefix = "";
});

app.UseStaticFiles(new StaticFileOptions{
    // Serve openapi.yaml file
    ServeUnknownFileTypes = true,
});

// AC-FOUNDATION-003.8, 004.7: Health endpoints unauthenticated
app.MapGet("/health", () => Results.Ok()).AllowAnonymous();
var healthStatusMap = new Dictionary<HealthStatus, int>
{
    [HealthStatus.Healthy] = StatusCodes.Status200OK,
    [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
    [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable,
};
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains(ApplicationHealthTags.Live),
    ResponseWriter = HealthReportJsonWriter.WriteLivenessAsync,
    ResultStatusCodes = healthStatusMap,
}).AllowAnonymous();
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains(ApplicationHealthTags.Ready),
    ResponseWriter = HealthReportJsonWriter.WriteReadinessAsync,
    ResultStatusCodes = healthStatusMap,
}).AllowAnonymous();
app.MapGet("/", () => Results.Ok("OK")).AllowAnonymous();

app.MapControllers();

// Protected API endpoints: add .RequireAuthorization() or .RequireAuthorization("Admin") when adding authenticated routes (AC-FOUNDATION-003.4, 003.5)

// AC-FOUNDATION-011.7: Initialize resilience event logging (retry, circuit breaker, timeout).
Todo.Api.Infrastructure.Resilience.ResilienceLogging.Initialize(app.Services.GetRequiredService<ILoggerFactory>());

app.Run();

// Expose for WebApplicationFactory in integration tests (AC-FOUNDATION-012.3).
public partial class Program { }

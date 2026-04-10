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
using Todo.Api.Application.Services;
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

// WO-41: Azure Event Hubs strongly-typed options
builder.Services.Configure<Todo.Api.Infrastructure.Configuration.EventHubSettings>(
    builder.Configuration.GetSection(Todo.Api.Infrastructure.Configuration.EventHubSettings.SectionName));

// WO-48: Event publisher — EventHubPublisher when configured, NoOp fallback for local dev
var ehNamespace = builder.Configuration["EventHubs:FullyQualifiedNamespace"];
if (!string.IsNullOrWhiteSpace(ehNamespace) && ehNamespace != "localhost")
{
    builder.Services.AddSingleton<Todo.Api.Application.EventPublishing.IEventPublisher,
        Todo.Api.Infrastructure.EventPublishing.EventHubPublisher>();
}
else
{
    builder.Services.AddSingleton<Todo.Api.Application.EventPublishing.IEventPublisher,
        Todo.Api.Infrastructure.EventPublishing.NoOpEventPublisher>();
}

// WO-17: Credential encryption (AES-256-GCM with per-tenant HKDF key derivation)
builder.Services.Configure<Todo.Api.Infrastructure.Security.CredentialEncryptionOptions>(
    builder.Configuration.GetSection(Todo.Api.Infrastructure.Security.CredentialEncryptionOptions.SectionName));
builder.Services.AddSingleton<Todo.Api.Application.Services.ICredentialEncryptionService,
    Todo.Api.Infrastructure.Security.CredentialEncryptionService>();

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
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<ITenantService, TenantService>();
    builder.Services.AddScoped<IUserManagementService, UserManagementService>();
    // WO-42: Business Plan CRUD API
    builder.Services.AddScoped<IBusinessPlanService, BusinessPlanService>();
    // WO-45: Connection CRUD and Test API
    builder.Services.AddScoped<IConnectionService, ConnectionService>();
    // WO-43: Pipeline Registration CRUD API
    builder.Services.AddScoped<IPipelineRegistrationService, PipelineRegistrationService>();
    // WO-44: Pipeline Lineage Relationship API
    builder.Services.AddScoped<ILineageService, LineageService>();
    // WO-46: Monitor CRUD API
    builder.Services.AddScoped<IMonitorService, MonitorService>();
    // WO-46: Query Template CRUD API
    builder.Services.AddScoped<IQueryTemplateService, QueryTemplateService>();
    // WO-47: Connector CRUD API
    builder.Services.AddSingleton<Todo.Api.Application.Connectors.ConnectorTypeCatalog>();
    builder.Services.AddScoped<IConnectorService, ConnectorService>();
    // WO-48: Connector adapters (all 13 stubs)
    builder.Services.AddScoped<Todo.Api.Application.Connectors.IConnectorAdapter, Todo.Api.Infrastructure.Connectors.Adapters.AirflowAdapter>();
    builder.Services.AddScoped<Todo.Api.Application.Connectors.IConnectorAdapter, Todo.Api.Infrastructure.Connectors.Adapters.TalendAdapter>();
    builder.Services.AddScoped<Todo.Api.Application.Connectors.IConnectorAdapter, Todo.Api.Infrastructure.Connectors.Adapters.HvrAdapter>();
    builder.Services.AddScoped<Todo.Api.Application.Connectors.IConnectorAdapter, Todo.Api.Infrastructure.Connectors.Adapters.MemSqlAdapter>();
    builder.Services.AddScoped<Todo.Api.Application.Connectors.IConnectorAdapter, Todo.Api.Infrastructure.Connectors.Adapters.ServiceNowAdapter>();
    builder.Services.AddScoped<Todo.Api.Application.Connectors.IConnectorAdapter, Todo.Api.Infrastructure.Connectors.Adapters.DatadogAdapter>();
    builder.Services.AddScoped<Todo.Api.Application.Connectors.IConnectorAdapter, Todo.Api.Infrastructure.Connectors.Adapters.NewRelicAdapter>();
    builder.Services.AddScoped<Todo.Api.Application.Connectors.IConnectorAdapter, Todo.Api.Infrastructure.Connectors.Adapters.DynatraceAdapter>();
    builder.Services.AddScoped<Todo.Api.Application.Connectors.IConnectorAdapter, Todo.Api.Infrastructure.Connectors.Adapters.PostgreSqlAdapter>();
    builder.Services.AddScoped<Todo.Api.Application.Connectors.IConnectorAdapter, Todo.Api.Infrastructure.Connectors.Adapters.OracleAdapter>();
    builder.Services.AddScoped<Todo.Api.Application.Connectors.IConnectorAdapter, Todo.Api.Infrastructure.Connectors.Adapters.SqlServerAdapter>();
    builder.Services.AddScoped<Todo.Api.Application.Connectors.IConnectorAdapter, Todo.Api.Infrastructure.Connectors.Adapters.AzureSynapseAdapter>();
    builder.Services.AddScoped<Todo.Api.Application.Connectors.IConnectorAdapter, Todo.Api.Infrastructure.Connectors.Adapters.CustomWebhookAdapter>();
    // WO-48: Connector health tracker and execution engine
    builder.Services.AddScoped<Todo.Api.Infrastructure.Connectors.ConnectorHealthTracker>();
    builder.Services.AddHostedService<Todo.Api.Infrastructure.Connectors.ConnectorExecutionEngine>();
    // WO-26: Kafka event processor + event handlers
    builder.Services.Configure<Todo.Api.Infrastructure.EventProcessing.KafkaSettings>(
        builder.Configuration.GetSection(Todo.Api.Infrastructure.EventProcessing.KafkaSettings.SectionName));
    builder.Services.AddScoped<Todo.Api.Infrastructure.EventProcessing.IEventHandler, Todo.Api.Infrastructure.EventProcessing.PipelineExecutionEventHandler>();
    builder.Services.AddScoped<Todo.Api.Infrastructure.EventProcessing.IEventHandler, Todo.Api.Infrastructure.EventProcessing.JobRunEventHandler>();
    builder.Services.AddScoped<Todo.Api.Infrastructure.EventProcessing.IEventHandler, Todo.Api.Infrastructure.EventProcessing.MemSQLInterfaceEventHandler>();
    // WO-27: Quality and infrastructure event handlers
    builder.Services.AddScoped<Todo.Api.Infrastructure.EventProcessing.IEventHandler, Todo.Api.Infrastructure.EventProcessing.DataQualityEventHandler>();
    builder.Services.AddScoped<Todo.Api.Infrastructure.EventProcessing.IEventHandler, Todo.Api.Infrastructure.EventProcessing.InfrastructureMetricEventHandler>();
    builder.Services.AddHostedService<Todo.Api.Infrastructure.EventProcessing.EventProcessorBackgroundService>();
    // WO-28: SLA Evaluation Job
    builder.Services.AddScoped<ISLAEvaluationService, SLAEvaluationService>();
    builder.Services.AddHostedService<Todo.Api.Infrastructure.BackgroundJobs.SLAEvaluationJob>();
    // WO-29: Data Quality Evaluation Job
    builder.Services.AddScoped<IDataQualityEvaluationService, DataQualityEvaluationService>();
    builder.Services.AddHostedService<Todo.Api.Infrastructure.BackgroundJobs.DataQualityEvaluationJob>();
    // WO-30: Infrastructure Health Evaluation + Long-Run Threshold Jobs
    builder.Services.AddScoped<IInfraHealthEvaluationService, InfraHealthEvaluationService>();
    builder.Services.AddHostedService<Todo.Api.Infrastructure.BackgroundJobs.InfraHealthEvaluationJob>();
    builder.Services.AddHostedService<Todo.Api.Infrastructure.BackgroundJobs.LongRunThresholdJob>();
    builder.Services.AddScoped<Todo.Api.Infrastructure.EventProcessing.IEventHandler, Todo.Api.Infrastructure.EventProcessing.HeartbeatEventHandler>();
    // WO-31: Pipeline Monitoring API
    builder.Services.AddScoped<IPipelineMonitoringService, PipelineMonitoringService>();
    // WO-32: Job Execution Monitoring API
    builder.Services.AddScoped<IJobMonitoringService, JobMonitoringService>();
    // WO-33: Data Quality and Latency API
    builder.Services.AddScoped<IDataQualityService, DataQualityService>();
    // WO-34: SLA Tracking API
    builder.Services.AddScoped<ISLAService, SLAService>();
    // WO-35: Infrastructure Monitoring API
    builder.Services.AddScoped<IInfrastructureMonitoringService, InfrastructureMonitoringService>();
}
else
{
    // WO-8: GET /api/v1/auth/me without Cosmos returns claims-only profile for local development.
    builder.Services.AddScoped<IAuthService, Todo.Api.Infrastructure.Identity.ClaimsOnlyAuthService>();
    // WO-9: tenant CRUD requires Cosmos; stub throws 503 via HttpRequestException when invoked.
    builder.Services.AddScoped<ITenantService, UnavailableTenantService>();
    builder.Services.AddScoped<IUserManagementService, UnavailableUserManagementService>();
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

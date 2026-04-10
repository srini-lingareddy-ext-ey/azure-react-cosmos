using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Todo.Api.Infrastructure.Data;
using Monitor = Todo.Api.Domain.Entities.Monitor;

namespace Todo.Api.Infrastructure.Configuration;

/// <summary>
/// DI registration for Cosmos DB client (session consistency, RU monitoring) and repositories (AC-FOUNDATION-002.6, 002.7).
/// </summary>
public static class CosmosServiceCollectionExtensions
{
    /// <summary>
    /// Adds Cosmos DB client with session consistency and optional RU monitoring.
    /// Only registers when AZURE_COSMOS_ENDPOINT is set.
    /// Uses <c>AZURE_COSMOS_KEY</c> when set (account key); otherwise <see cref="DefaultAzureCredential"/> (managed identity or Azure CLI locally).
    /// </summary>
    public static IServiceCollection AddCosmosDbClient(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["AZURE_COSMOS_ENDPOINT"];
        if (string.IsNullOrEmpty(endpoint))
            return services;

        var options = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            },
            ConsistencyLevel = ConsistencyLevel.Session,
            ApplicationRegion = configuration["AZURE_LOCATION"] ?? null,
        };

        // Local dev: set AZURE_COSMOS_KEY (user-secrets or env). Azure: omit key and use DefaultAzureCredential.
        var accountKey = configuration["AZURE_COSMOS_KEY"];
        CosmosClient client = !string.IsNullOrWhiteSpace(accountKey)
            ? new CosmosClient(endpoint, accountKey, options)
            : new CosmosClient(endpoint, new DefaultAzureCredential(), options);
        services.AddSingleton(client);
        return services;
    }

    /// <summary>
    /// Registers <see cref="IRepository{T}"/> with Cosmos DB implementation for the given database, container, and partition key path.
    /// </summary>
    /// <typeparam name="T">Entity type (must implement <see cref="IDomainEntity"/>).</typeparam>
    /// <param name="databaseId">Cosmos database id.</param>
    /// <param name="containerId">Container id.</param>
    /// <param name="partitionKeyPath">Partition key path (e.g. "/partitionKey").</param>
    public static IServiceCollection AddCosmosDbRepository<T>(
        this IServiceCollection services,
        string databaseId,
        string containerId,
        string partitionKeyPath) where T : class, IDomainEntity
    {
        services.AddSingleton<IRepository<T>>(sp =>
        {
            var client = sp.GetRequiredService<CosmosClient>();
            var logger = sp.GetRequiredService<ILogger<CosmosDbRepositoryBase<T>>>();
            var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
            return new CosmosDbRepositoryBase<T>(client, databaseId, containerId, partitionKeyPath, logger, httpContextAccessor);
        });
        return services;
    }

    /// <summary>
    /// Registers tenant, user-role-assignment, and user-invitation repositories for the given database (WO-4 / WO-5 and invitations).
    /// </summary>
    public static IServiceCollection AddAppCosmosRepositories(this IServiceCollection services, string databaseId)
    {
        services.AddCosmosDbRepository<Tenant>(databaseId, "tenant", "/id");
        services.AddSingleton<ITenantRepository, TenantRepository>();

        services.AddCosmosDbRepository<UserRoleAssignment>(databaseId, "user-role-assignment", "/tenantId");
        services.AddSingleton<IUserRoleAssignmentRepository, UserRoleAssignmentRepository>();

        services.AddCosmosDbRepository<UserInvitation>(databaseId, "user-invitation", "/tenantId");
        services.AddSingleton<IUserInvitationRepository, UserInvitationRepository>();

        // Phase 3: domain entity repositories (WO-15 through WO-20)
        services.AddCosmosDbRepository<BusinessPlan>(databaseId, "business-plan", "/tenantId");
        services.AddSingleton<IBusinessPlanRepository, BusinessPlanRepository>();

        services.AddCosmosDbRepository<PipelineRegistration>(databaseId, "pipeline-registration", "/tenantId");
        services.AddSingleton<IPipelineRegistrationRepository, PipelineRegistrationRepository>();

        services.AddCosmosDbRepository<PipelineLineageRelationship>(databaseId, "pipeline-lineage-relationship", "/tenantId");
        services.AddSingleton<IPipelineLineageRepository, PipelineLineageRepository>();

        services.AddCosmosDbRepository<Connection>(databaseId, "connection", "/tenantId");
        services.AddSingleton<IConnectionRepository, ConnectionRepository>();

        services.AddCosmosDbRepository<QueryTemplate>(databaseId, "query-template", "/tenantId");
        services.AddSingleton<IQueryTemplateRepository, QueryTemplateRepository>();

        services.AddCosmosDbRepository<Monitor>(databaseId, "monitor", "/tenantId");
        services.AddSingleton<IMonitorRepository, MonitorRepository>();

        services.AddCosmosDbRepository<MonitorStatus>(databaseId, "monitor-status", "/tenantId");
        services.AddSingleton<IMonitorStatusRepository, MonitorStatusRepository>();

        services.AddCosmosDbRepository<ConnectorInstance>(databaseId, "connector-instance", "/tenantId");
        services.AddSingleton<IConnectorInstanceRepository, ConnectorInstanceRepository>();

        services.AddCosmosDbRepository<ConnectorHealthStatus>(databaseId, "connector-health-status", "/tenantId");
        services.AddSingleton<IConnectorHealthStatusRepository, ConnectorHealthStatusRepository>();

        services.AddCosmosDbRepository<ConnectorExecutionLog>(databaseId, "connector-execution-log", "/tenantId");
        services.AddSingleton<IConnectorExecutionLogRepository, ConnectorExecutionLogRepository>();

        // Phase 4: Pipeline Monitoring (WO-21)
        services.AddCosmosDbRepository<PipelineStatusSummary>(databaseId, "pipeline-status-summary", "/tenantId");
        services.AddSingleton<IPipelineStatusSummaryRepository, PipelineStatusSummaryRepository>();

        services.AddCosmosDbRepository<PipelineExecution>(databaseId, "pipeline-execution", "/tenantId");
        services.AddSingleton<IPipelineExecutionRepository, PipelineExecutionRepository>();

        services.AddCosmosDbRepository<MemSQLInterfaceStatus>(databaseId, "memsql-interface-status", "/tenantId");
        services.AddSingleton<IMemSQLInterfaceStatusRepository, MemSQLInterfaceStatusRepository>();

        // Phase 4: Job Monitoring (WO-22) — job-run has TTL enabled
        services.AddCosmosDbRepository<JobRun>(databaseId, "job-run", "/tenantId");
        services.AddSingleton<IJobRunRepository, JobRunRepository>();

        services.AddCosmosDbRepository<JobLongRunThreshold>(databaseId, "job-long-run-threshold", "/tenantId");
        services.AddSingleton<IJobLongRunThresholdRepository, JobLongRunThresholdRepository>();

        // Phase 4: Data Quality (WO-23)
        services.AddCosmosDbRepository<DataQualityScore>(databaseId, "data-quality-score", "/tenantId");
        services.AddSingleton<IDataQualityScoreRepository, DataQualityScoreRepository>();

        services.AddCosmosDbRepository<DataQualityStatus>(databaseId, "data-quality-status", "/tenantId");
        services.AddSingleton<IDataQualityStatusRepository, DataQualityStatusRepository>();

        services.AddCosmosDbRepository<DataQualityThresholdConfig>(databaseId, "data-quality-threshold-config", "/tenantId");
        services.AddSingleton<IDataQualityThresholdConfigRepository, DataQualityThresholdConfigRepository>();

        // Phase 4: SLA Tracking (WO-24)
        services.AddCosmosDbRepository<PipelineSLAConfig>(databaseId, "pipeline-sla-config", "/tenantId");
        services.AddSingleton<IPipelineSLAConfigRepository, PipelineSLAConfigRepository>();

        services.AddCosmosDbRepository<PipelineSLAStatus>(databaseId, "pipeline-sla-status", "/tenantId");
        services.AddSingleton<IPipelineSLAStatusRepository, PipelineSLAStatusRepository>();

        services.AddCosmosDbRepository<PipelineSLABreachRecord>(databaseId, "pipeline-sla-breach-record", "/tenantId");
        services.AddSingleton<IPipelineSLABreachRecordRepository, PipelineSLABreachRecordRepository>();

        // Phase 4: Infrastructure Monitoring (WO-25) — infrastructure-metric has TTL enabled
        services.AddCosmosDbRepository<ComponentHealthStatus>(databaseId, "component-health-status", "/tenantId");
        services.AddSingleton<IComponentHealthStatusRepository, ComponentHealthStatusRepository>();

        services.AddCosmosDbRepository<NodeHealthStatus>(databaseId, "node-health-status", "/tenantId");
        services.AddSingleton<INodeHealthStatusRepository, NodeHealthStatusRepository>();

        services.AddCosmosDbRepository<InfrastructureMetric>(databaseId, "infrastructure-metric", "/tenantId");
        services.AddSingleton<IInfrastructureMetricRepository, InfrastructureMetricRepository>();

        services.AddCosmosDbRepository<InfraThresholdConfig>(databaseId, "infra-threshold-config", "/tenantId");
        services.AddSingleton<IInfraThresholdConfigRepository, InfraThresholdConfigRepository>();

        services.AddCosmosDbRepository<ProductAvailability>(databaseId, "product-availability", "/tenantId");
        services.AddSingleton<IProductAvailabilityRepository, ProductAvailabilityRepository>();

        // Phase 5: Event Intelligence (WO-52)
        services.AddCosmosDbRepository<Event>(databaseId, "events", "/tenantId");
        services.AddSingleton<IEventRepository, EventRepository>();

        services.AddCosmosDbRepository<ClassificationAuditEntry>(databaseId, "classification-audit", "/tenantId");
        services.AddSingleton<IClassificationAuditRepository, ClassificationAuditRepository>();

        // PurgeAuditEntry writes to existing audit-log container — no separate repo needed

        // Phase 5: Classification Rules (WO-53)
        services.AddCosmosDbRepository<ClassificationRule>(databaseId, "classification-rules", "/tenantId");
        services.AddSingleton<IClassificationRuleRepository, ClassificationRuleRepository>();

        // Phase 5: Change Fingerprinting (WO-54)
        services.AddCosmosDbRepository<MonitoredArtifact>(databaseId, "monitored-artifact", "/tenantId");
        services.AddSingleton<IMonitoredArtifactRepository, MonitoredArtifactRepository>();

        services.AddCosmosDbRepository<ArtifactFingerprint>(databaseId, "artifact-fingerprint", "/tenantId");
        services.AddSingleton<IArtifactFingerprintRepository, ArtifactFingerprintRepository>();

        services.AddCosmosDbRepository<FingerprintApprovedWindow>(databaseId, "fingerprint-approved-window", "/tenantId");
        services.AddSingleton<IFingerprintApprovedWindowRepository, FingerprintApprovedWindowRepository>();

        services.AddCosmosDbRepository<FingerprintAuditEntry>(databaseId, "fingerprint-audit", "/tenantId");
        services.AddSingleton<IFingerprintAuditRepository, FingerprintAuditRepository>();

        return services;
    }
}

using Azure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Todo.Api.Infrastructure.Data;

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
        return services;
    }
}

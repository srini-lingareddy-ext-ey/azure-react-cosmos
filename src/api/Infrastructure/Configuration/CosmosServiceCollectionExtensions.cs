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
    /// </summary>
    public static IServiceCollection AddCosmosDbClient(this IServiceCollection services, IConfiguration configuration)
    {
        var endpoint = configuration["AZURE_COSMOS_ENDPOINT"];
        if (string.IsNullOrEmpty(endpoint))
            return services;

        var credential = new DefaultAzureCredential();
        var options = new CosmosClientOptions
        {
            SerializerOptions = new CosmosSerializationOptions
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            },
            ConsistencyLevel = ConsistencyLevel.Session,
            ApplicationRegion = configuration["AZURE_LOCATION"] ?? null,
        };
        var client = new CosmosClient(endpoint, credential, options);
        services.AddSingleton(client);
        return services;
    }

    /// <summary>
    /// Registers <see cref="IRepository{T}"/> with Cosmos DB implementation for the given database, container, and partition key path.
    /// </summary>
    /// <typeparam name="T">Entity type (must implement Domain.Entities.IDomainEntity).</typeparam>
    /// <param name="databaseId">Cosmos database id.</param>
    /// <param name="containerId">Container id.</param>
    /// <param name="partitionKeyPath">Partition key path (e.g. "/partitionKey").</param>
    public static IServiceCollection AddCosmosDbRepository<T>(
        this IServiceCollection services,
        string databaseId,
        string containerId,
        string partitionKeyPath) where T : class, Domain.Entities.IDomainEntity
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
    /// WO-4 / WO-5: registers <see cref="IRepository{Tenant}"/>, <see cref="ITenantRepository"/>,
    /// <see cref="IRepository{UserRoleAssignment}"/>, and <see cref="IUserRoleAssignmentRepository"/> for the given database.
    /// </summary>
    public static IServiceCollection AddAppCosmosRepositories(this IServiceCollection services, string databaseId)
    {
        services.AddCosmosDbRepository<Tenant>(databaseId, "tenant", "/id");
        services.AddSingleton<ITenantRepository, TenantRepository>();

        services.AddCosmosDbRepository<UserRoleAssignment>(databaseId, "user-role-assignment", "/tenantId");
        services.AddSingleton<IUserRoleAssignmentRepository, UserRoleAssignmentRepository>();
        return services;
    }
}

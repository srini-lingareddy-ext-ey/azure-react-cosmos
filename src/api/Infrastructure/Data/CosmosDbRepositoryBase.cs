using System.Net;
using System.Security.Claims;
using Microsoft.Azure.Cosmos;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Exceptions;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>
/// Base Cosmos DB repository: CRUD, query, ETag handling, audit fields (AC-FOUNDATION-002.2–002.6).
/// All methods are async-only. Request charge is logged for RU monitoring.
/// CreatedBy/UpdatedBy use the standardized audit identity: oid (Object ID) primary, fallback sub (AC-FOUNDATION-003.7).
/// </summary>
public sealed class CosmosDbRepositoryBase<T> : IRepository<T> where T : class, IDomainEntity
{
    private readonly Container _container;
    private readonly ILogger<CosmosDbRepositoryBase<T>> _logger;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public CosmosDbRepositoryBase(
        CosmosClient client,
        string databaseId,
        string containerId,
        string partitionKeyPath,
        ILogger<CosmosDbRepositoryBase<T>> logger,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _container = client.GetContainer(databaseId, containerId);
    }

    private static PartitionKey ToPartitionKey(object value) =>
        value is null ? PartitionKey.None : new PartitionKey(value.ToString());

    /// <inheritdoc />
    public async Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default)
    {
        PopulateAuditOnCreate(entity);
        var response = await _container.CreateItemAsync(
            entity,
            ToPartitionKey(entity.PartitionKeyValue),
            cancellationToken: cancellationToken).ConfigureAwait(false);
        LogRequestCharge("Create", response.RequestCharge);
        SetEtagFromResponse(entity, response.ETag);
        return response.Resource;
    }

    /// <inheritdoc />
    public async Task<T?> GetByIdAsync(string id, object partitionKeyValue, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _container.ReadItemAsync<T>(
                id,
                ToPartitionKey(partitionKeyValue),
                cancellationToken: cancellationToken).ConfigureAwait(false);
            LogRequestCharge("Read", response.RequestCharge);
            var resource = response.Resource;
            SetEtagFromResponse(resource, response.ETag);
            return resource!;
        }
        catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc />
    /// <exception cref="ConcurrencyConflictException">Thrown when the entity's ETag no longer matches the stored document (Cosmos 412 Precondition Failed).</exception>
    public async Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        PopulateAuditOnUpdate(entity);
        var options = BuildRequestOptionsForUpdate(entity);
        try
        {
            var response = await _container.ReplaceItemAsync(
                entity,
                entity.Id,
                ToPartitionKey(entity.PartitionKeyValue),
                options,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            LogRequestCharge("Update", response.RequestCharge);
            SetEtagFromResponse(entity, response.ETag);
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            throw new ConcurrencyConflictException("The resource was modified by another process. Refresh and retry.", ex);
        }
    }

    /// <inheritdoc />
    public async Task<T> UpsertAsync(T entity, CancellationToken cancellationToken = default)
    {
        PopulateAuditOnUpdate(entity);
        if (entity is IAuditableEntity auditable && auditable.CreatedAt is null)
            PopulateAuditOnCreate(entity);
        var response = await _container.UpsertItemAsync(
            entity,
            ToPartitionKey(entity.PartitionKeyValue),
            cancellationToken: cancellationToken).ConfigureAwait(false);
        LogRequestCharge("Upsert", response.RequestCharge);
        SetEtagFromResponse(entity, response.ETag);
        return response.Resource;
    }

    /// <inheritdoc />
    /// <exception cref="ConcurrencyConflictException">Thrown when <paramref name="etag"/> is supplied and no longer matches the stored document (Cosmos 412 Precondition Failed).</exception>
    public async Task DeleteAsync(string id, object partitionKeyValue, string? etag = null, CancellationToken cancellationToken = default)
    {
        var options = new ItemRequestOptions();
        if (!string.IsNullOrEmpty(etag))
            options.IfMatchEtag = etag;
        try
        {
            var response = await _container.DeleteItemAsync<T>(
                id,
                ToPartitionKey(partitionKeyValue),
                options,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            LogRequestCharge("Delete", response.RequestCharge);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.PreconditionFailed)
        {
            throw new ConcurrencyConflictException("The resource was modified by another process. Refresh and retry.", ex);
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<T> QueryAsync(QuerySpec spec, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = new QueryDefinition(spec.QueryText);
        if (spec.Parameters is { Count: > 0 })
        {
            foreach (var p in spec.Parameters)
                query = query.WithParameter(p.Key, p.Value);
        }
        using var iterator = _container.GetItemQueryIterator<T>(query);
        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            LogRequestCharge("Query", response.RequestCharge);
            foreach (var item in response)
                yield return item;
        }
    }

    private void PopulateAuditOnCreate(T entity)
    {
        if (entity is not IAuditableEntity auditable) return;
        var now = DateTimeOffset.UtcNow;
        auditable.CreatedAt = now;
        auditable.UpdatedAt = now;
        var userId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            auditable.CreatedBy = userId;
            auditable.UpdatedBy = userId;
        }
    }

    /// <summary>Sets only UpdatedAt and UpdatedBy. Never overwrites CreatedAt or CreatedBy (AC-FOUNDATION-002.5).</summary>
    private void PopulateAuditOnUpdate(T entity)
    {
        if (entity is not IAuditableEntity auditable) return;
        auditable.UpdatedAt = DateTimeOffset.UtcNow;
        var userId = GetCurrentUserId();
        if (!string.IsNullOrEmpty(userId))
            auditable.UpdatedBy = userId;
    }

    // Audit identity: oid (Object ID) primary, fallback sub — must match CurrentUserService and docs.
    private string? GetCurrentUserId() =>
        _httpContextAccessor?.HttpContext?.User?.FindFirstValue("oid")
        ?? _httpContextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? _httpContextAccessor?.HttpContext?.User?.FindFirstValue("sub");

    private ItemRequestOptions? BuildRequestOptionsForUpdate(T entity)
    {
        if (entity is not IConcurrencyEntity concurrency || string.IsNullOrEmpty(concurrency.Etag))
            return null;
        return new ItemRequestOptions { IfMatchEtag = concurrency.Etag };
    }

    private static void SetEtagFromResponse(T? entity, string? etag)
    {
        if (entity is IConcurrencyEntity concurrency && !string.IsNullOrEmpty(etag))
            concurrency.Etag = etag;
    }

    /// <summary>Logs request charge at Debug. RU monitoring is logging-only; structured telemetry (e.g. Application Insights metrics) can be added via options if needed (AC-FOUNDATION-002.6).</summary>
    private void LogRequestCharge(string operation, double requestCharge)
    {
        if (requestCharge > 0)
            _logger.LogDebug("Cosmos DB {Operation} request charge: {RequestCharge} RUs", operation, requestCharge);
    }
}

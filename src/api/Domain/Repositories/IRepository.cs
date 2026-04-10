using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Exceptions;

namespace Todo.Api.Domain.Repositories;

/// <summary>
/// Repository interface for entity persistence (AC-FOUNDATION-002.1, 002.2).
/// All operations are async-only (AC-FOUNDATION-002.3).
/// Implementations handle ETag for optimistic concurrency and populate audit fields.
/// </summary>
/// <typeparam name="T">Entity type; must implement <see cref="IDomainEntity"/>.</typeparam>
public interface IRepository<T> where T : class, IDomainEntity
{
    /// <summary>Creates the entity. Audit fields are set by the repository.</summary>
    Task<T> CreateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Reads a single item by id and partition key. Returns null if not found.</summary>
    Task<T?> GetByIdAsync(string id, object partitionKeyValue, CancellationToken cancellationToken = default);

    /// <summary>Updates the entity. Uses ETag for optimistic concurrency when the entity implements <see cref="IConcurrencyEntity"/> with a non-null Etag. On ETag mismatch, throws <see cref="ConcurrencyConflictException"/> (Cosmos 412).</summary>
    /// <exception cref="ConcurrencyConflictException">Thrown when the stored document was modified since it was read (ETag conflict).</exception>
    Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Deletes by id and partition key. When <paramref name="etag"/> is supplied, delete is conditional; on mismatch throws <see cref="ConcurrencyConflictException"/> (Cosmos 412).</summary>
    /// <exception cref="ConcurrencyConflictException">Thrown when <paramref name="etag"/> was supplied and the stored document was modified since (ETag conflict).</exception>
    Task DeleteAsync(string id, object partitionKeyValue, string? etag = null, CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces the entity (Cosmos DB upsert). Audit fields are set by the repository.</summary>
    Task<T> UpsertAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Executes a query and returns matching items. Query text and parameter names are defined by the implementation (e.g. Cosmos SQL with @params).</summary>
    IAsyncEnumerable<T> QueryAsync(QuerySpec spec, CancellationToken cancellationToken = default);
}

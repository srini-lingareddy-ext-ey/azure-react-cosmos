using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>
/// Cosmos-backed user–tenant role assignments (WO-5). Composes <see cref="IRepository{UserRoleAssignment}"/> (partition key /tenantId).
/// </summary>
public sealed class UserRoleAssignmentRepository : IUserRoleAssignmentRepository
{
    private readonly IRepository<UserRoleAssignment> _repository;

    public UserRoleAssignmentRepository(IRepository<UserRoleAssignment> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<UserRoleAssignment?> GetByUserAndTenantAsync(
        string userId,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec(
            "SELECT * FROM c WHERE c.userId = @userId AND c.tenantId = @tenantId",
            new Dictionary<string, object>
            {
                ["@userId"] = userId,
                ["@tenantId"] = tenantId,
            });
        await foreach (var row in _repository.QueryAsync(spec, cancellationToken).ConfigureAwait(false))
            return row;
        return null;
    }

    /// <inheritdoc />
    public IAsyncEnumerable<UserRoleAssignment> GetAllByTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec(
            "SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    /// <inheritdoc />
    public IAsyncEnumerable<UserRoleAssignment> GetAllByUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec(
            "SELECT * FROM c WHERE c.userId = @userId",
            new Dictionary<string, object> { ["@userId"] = userId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<UserRoleAssignment> CreateAsync(
        UserRoleAssignment assignment,
        CancellationToken cancellationToken = default)
    {
        if (assignment.SchemaVersion == 0)
            assignment.SchemaVersion = 1;
        return await _repository.CreateAsync(assignment, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<UserRoleAssignment> UpdateAsync(
        UserRoleAssignment assignment,
        CancellationToken cancellationToken = default) =>
        _repository.UpdateAsync(assignment, cancellationToken);
}

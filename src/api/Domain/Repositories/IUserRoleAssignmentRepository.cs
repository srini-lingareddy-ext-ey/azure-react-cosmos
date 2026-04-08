using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

/// <summary>Persistence for <see cref="UserRoleAssignment"/> (WO-5).</summary>
public interface IUserRoleAssignmentRepository
{
    /// <summary>
    /// Returns the assignment for the user in the tenant, or null. Uses a Cosmos SQL query (may fan out if not partition-scoped at the SDK layer).
    /// </summary>
    Task<UserRoleAssignment?> GetByUserAndTenantAsync(string userId, string tenantId, CancellationToken cancellationToken = default);

    /// <summary>All assignments in the tenant (e.g. admin roster).</summary>
    IAsyncEnumerable<UserRoleAssignment> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);

    /// <summary>All assignments for a user across tenants.</summary>
    IAsyncEnumerable<UserRoleAssignment> GetAllByUserAsync(string userId, CancellationToken cancellationToken = default);

    Task<UserRoleAssignment> CreateAsync(UserRoleAssignment assignment, CancellationToken cancellationToken = default);

    Task<UserRoleAssignment> UpdateAsync(UserRoleAssignment assignment, CancellationToken cancellationToken = default);
}

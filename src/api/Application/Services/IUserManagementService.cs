using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Services;

/// <summary>WO-10: tenant user roster and role/status management.</summary>
public interface IUserManagementService
{
    Task<UserRosterResponse> GetRosterAsync(
        string actorUserId,
        string tenantId,
        UserRole? roleFilter,
        UserStatus? statusFilter,
        int limit,
        int offset,
        CancellationToken cancellationToken = default);

    Task ChangeUserRoleAsync(
        string actorUserId,
        string tenantId,
        string targetUserId,
        UserRole newRole,
        CancellationToken cancellationToken = default);

    Task DeactivateUserAsync(
        string actorUserId,
        string tenantId,
        string targetUserId,
        CancellationToken cancellationToken = default);

    Task ActivateUserAsync(
        string actorUserId,
        string tenantId,
        string targetUserId,
        CancellationToken cancellationToken = default);

    /// <summary>WO-11: add user by id or create email invitation.</summary>
    Task<AddUserResponse> AddUserAsync(
        string actorUserId,
        string tenantId,
        AddUserRequest request,
        CancellationToken cancellationToken = default);
}

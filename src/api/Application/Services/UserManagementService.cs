using Microsoft.Extensions.Logging;
using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>
/// WO-10: roster and user role/status updates; authorization mirrors <see cref="TenantService"/> (PlatformAdmin or tenant Admin).
/// </summary>
public sealed class UserManagementService : IUserManagementService
{
    private const int DefaultLimit = 50;
    private const int MaxLimit = 100;

    private readonly IUserRoleAssignmentRepository _assignmentRepository;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        IUserRoleAssignmentRepository assignmentRepository,
        ILogger<UserManagementService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<UserRosterResponse> GetRosterAsync(
        string actorUserId,
        string tenantId,
        UserRole? roleFilter,
        UserStatus? statusFilter,
        int limit,
        int offset,
        CancellationToken cancellationToken = default)
    {
        await RequirePlatformAdminOrTenantAdminAsync(actorUserId, tenantId, cancellationToken).ConfigureAwait(false);

        limit = limit > 0 ? Math.Clamp(limit, 1, MaxLimit) : DefaultLimit;
        offset = Math.Max(0, offset);

        var all = new List<UserRoleAssignment>();
        await foreach (var a in _assignmentRepository.GetAllByTenantAsync(tenantId, cancellationToken).ConfigureAwait(false))
        {
            all.Add(a);
        }

        IEnumerable<UserRoleAssignment> query = all;
        if (roleFilter is { } rf)
        {
            query = query.Where(a => a.Role == rf);
        }

        if (statusFilter is { } sf)
        {
            query = query.Where(a => a.Status == sf);
        }

        var sorted = query
            .OrderBy(a => DisplaySortKey(a), StringComparer.OrdinalIgnoreCase)
            .ToList();
        var total = sorted.Count;
        var page = sorted
            .Skip(offset)
            .Take(limit)
            .Select(MapToUserResponse)
            .ToList();

        return new UserRosterResponse
        {
            Items = page,
            TotalCount = total,
            Limit = limit,
            Offset = offset,
        };
    }

    /// <inheritdoc />
    public async Task ChangeUserRoleAsync(
        string actorUserId,
        string tenantId,
        string targetUserId,
        UserRole newRole,
        CancellationToken cancellationToken = default)
    {
        await RequirePlatformAdminOrTenantAdminAsync(actorUserId, tenantId, cancellationToken).ConfigureAwait(false);

        if (string.Equals(actorUserId, targetUserId, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Administrators cannot change their own role.");
        }

        if (!Enum.IsDefined(newRole))
        {
            throw new ArgumentException("Role is not a valid value.", nameof(newRole));
        }

        var assignment = await _assignmentRepository
            .GetByUserAndTenantAsync(targetUserId, tenantId, cancellationToken)
            .ConfigureAwait(false);
        if (assignment is null)
        {
            throw new KeyNotFoundException($"User '{targetUserId}' was not found in this tenant.");
        }

        var previousRole = assignment.Role;
        if (previousRole == newRole)
        {
            return;
        }

        assignment.Role = newRole;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;
        assignment.UpdatedBy = actorUserId;
        await _assignmentRepository.UpdateAsync(assignment, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "User role changed from {PreviousRole} to {NewRole} for {TargetUserId} in tenant {TenantId} by {ChangedByUserId}.",
            previousRole,
            newRole,
            targetUserId,
            tenantId,
            actorUserId);
    }

    /// <inheritdoc />
    public async Task DeactivateUserAsync(
        string actorUserId,
        string tenantId,
        string targetUserId,
        CancellationToken cancellationToken = default)
    {
        await RequirePlatformAdminOrTenantAdminAsync(actorUserId, tenantId, cancellationToken).ConfigureAwait(false);

        var assignment = await _assignmentRepository
            .GetByUserAndTenantAsync(targetUserId, tenantId, cancellationToken)
            .ConfigureAwait(false);
        if (assignment is null)
        {
            throw new KeyNotFoundException($"User '{targetUserId}' was not found in this tenant.");
        }

        assignment.Status = UserStatus.Inactive;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;
        assignment.UpdatedBy = actorUserId;
        await _assignmentRepository.UpdateAsync(assignment, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ActivateUserAsync(
        string actorUserId,
        string tenantId,
        string targetUserId,
        CancellationToken cancellationToken = default)
    {
        await RequirePlatformAdminOrTenantAdminAsync(actorUserId, tenantId, cancellationToken).ConfigureAwait(false);

        var assignment = await _assignmentRepository
            .GetByUserAndTenantAsync(targetUserId, tenantId, cancellationToken)
            .ConfigureAwait(false);
        if (assignment is null)
        {
            throw new KeyNotFoundException($"User '{targetUserId}' was not found in this tenant.");
        }

        assignment.Status = UserStatus.Active;
        assignment.UpdatedAt = DateTimeOffset.UtcNow;
        assignment.UpdatedBy = actorUserId;
        await _assignmentRepository.UpdateAsync(assignment, cancellationToken).ConfigureAwait(false);
    }

    private static string DisplaySortKey(UserRoleAssignment a)
    {
        if (!string.IsNullOrWhiteSpace(a.DisplayName))
        {
            return a.DisplayName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(a.Email))
        {
            return a.Email.Trim();
        }

        return a.UserId;
    }

    private static UserResponse MapToUserResponse(UserRoleAssignment a)
    {
        return new UserResponse
        {
            UserId = a.UserId,
            DisplayName = a.DisplayName,
            Email = a.Email,
            Role = a.Role,
            Status = a.Status,
            LastLoginAt = a.LastLoginAt,
        };
    }

    private async Task RequirePlatformAdminOrTenantAdminAsync(
        string userId,
        string tenantId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException();
        }

        if (await IsPlatformAdminAsync(userId, cancellationToken).ConfigureAwait(false))
        {
            return;
        }

        var assignment = await _assignmentRepository
            .GetByUserAndTenantAsync(userId, tenantId, cancellationToken)
            .ConfigureAwait(false);
        if (assignment is null
            || assignment.Status != UserStatus.Active
            || assignment.Role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("This operation requires PlatformAdmin or Admin access to this tenant.");
        }
    }

    private async Task<bool> IsPlatformAdminAsync(string userId, CancellationToken cancellationToken)
    {
        await foreach (var a in _assignmentRepository.GetAllByUserAsync(userId, cancellationToken).ConfigureAwait(false))
        {
            if (a.Role == UserRole.PlatformAdmin && a.Status == UserStatus.Active)
            {
                return true;
            }
        }

        return false;
    }
}

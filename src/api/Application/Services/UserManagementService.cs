using Microsoft.Extensions.Logging;
using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Exceptions;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>
/// WO-10: roster and user role/status updates; authorization mirrors <see cref="TenantService"/> (PlatformAdmin or tenant Admin).
/// </summary>
public sealed class UserManagementService : IUserManagementService
{
    private const int DefaultLimit = 50;
    private const int MaxLimit = 100;

    private const int InvitationTtlSeconds = 2592000; // 30 days (WO-11 Cosmos TTL)
    private static readonly TimeSpan InvitationExpiry = TimeSpan.FromHours(72);

    private readonly IUserRoleAssignmentRepository _assignmentRepository;
    private readonly IUserInvitationRepository _invitationRepository;
    private readonly ILogger<UserManagementService> _logger;

    public UserManagementService(
        IUserRoleAssignmentRepository assignmentRepository,
        IUserInvitationRepository invitationRepository,
        ILogger<UserManagementService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _invitationRepository = invitationRepository;
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

    /// <inheritdoc />
    public async Task<AddUserResponse> AddUserAsync(
        string actorUserId,
        string tenantId,
        AddUserRequest request,
        CancellationToken cancellationToken = default)
    {
        await RequirePlatformAdminOrTenantAdminAsync(actorUserId, tenantId, cancellationToken).ConfigureAwait(false);

        if (!Enum.IsDefined(request.Role))
        {
            throw new ArgumentException("Role is not a valid value.", nameof(request));
        }

        if (!string.IsNullOrWhiteSpace(request.UserId))
        {
            return await AddUserByUserIdAsync(actorUserId, tenantId, request.UserId.Trim(), request.Role, cancellationToken)
                .ConfigureAwait(false);
        }

        return await AddUserByEmailAsync(actorUserId, tenantId, request.Email!.Trim(), request.Role, cancellationToken)
            .ConfigureAwait(false);
    }

    private async Task<AddUserResponse> AddUserByUserIdAsync(
        string actorUserId,
        string tenantId,
        string targetUserId,
        UserRole role,
        CancellationToken cancellationToken)
    {
        var existing = await _assignmentRepository
            .GetByUserAndTenantAsync(targetUserId, tenantId, cancellationToken)
            .ConfigureAwait(false);
        if (existing is { Status: UserStatus.Active })
        {
            throw new ActiveUserAssignmentConflictException(targetUserId, tenantId);
        }

        if (existing is not null)
        {
            existing.Role = role;
            existing.Status = UserStatus.Active;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
            existing.UpdatedBy = actorUserId;
            var updated = await _assignmentRepository.UpdateAsync(existing, cancellationToken).ConfigureAwait(false);
            return new AddUserResponse { Kind = "assignment", Id = updated.Id };
        }

        var assignment = new UserRoleAssignment
        {
            Id = Guid.NewGuid().ToString("n"),
            TenantId = tenantId,
            UserId = targetUserId,
            Role = role,
            Status = UserStatus.Active,
            SchemaVersion = 1,
        };
        var created = await _assignmentRepository.CreateAsync(assignment, cancellationToken).ConfigureAwait(false);
        return new AddUserResponse { Kind = "assignment", Id = created.Id };
    }

    private async Task<AddUserResponse> AddUserByEmailAsync(
        string actorUserId,
        string tenantId,
        string email,
        UserRole role,
        CancellationToken cancellationToken)
    {
        await foreach (var a in _assignmentRepository.GetAllByTenantAsync(tenantId, cancellationToken).ConfigureAwait(false))
        {
            if (a.Status == UserStatus.Active
                && !string.IsNullOrWhiteSpace(a.Email)
                && string.Equals(a.Email.Trim(), email, StringComparison.OrdinalIgnoreCase))
            {
                throw new ActiveUserAssignmentConflictException(a.UserId, tenantId);
            }
        }

        var prior = await _invitationRepository
            .GetByEmailAndTenantAsync(email, tenantId, cancellationToken)
            .ConfigureAwait(false);
        if (prior is { Status: InvitationStatus.Pending } && prior.ExpiresAt > DateTimeOffset.UtcNow)
        {
            throw new PendingInvitationConflictException(email, tenantId);
        }

        var now = DateTimeOffset.UtcNow;
        var expiresAt = now.Add(InvitationExpiry);
        var invitation = new UserInvitation
        {
            Id = Guid.NewGuid().ToString("n"),
            TenantId = tenantId,
            Email = email,
            Role = role,
            InvitedBy = actorUserId,
            InvitedAt = now,
            ExpiresAt = expiresAt,
            Status = InvitationStatus.Pending,
            AcceptedAt = null,
            Ttl = InvitationTtlSeconds,
            SchemaVersion = 1,
        };

        var created = await _invitationRepository.CreateAsync(invitation, cancellationToken).ConfigureAwait(false);

        _logger.LogInformation(
            "INVITE_STUB TenantId={TenantId} Email={Email} Role={Role} ExpiresAt={ExpiresAt}",
            tenantId,
            email,
            role,
            expiresAt);

        return new AddUserResponse { Kind = "invitation", Id = created.Id };
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

using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>
/// WO-8: resolves <see cref="UserProfileResponse"/> from Cosmos assignments and tenants.
/// <see cref="IUserRoleAssignmentRepository.GetAllByUserAsync"/> is a cross-partition query on the user-role-assignment container — monitor RU usage in production.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IUserRoleAssignmentRepository _assignmentRepo;
    private readonly ITenantRepository _tenantRepo;

    public AuthService(
        IUserRoleAssignmentRepository assignmentRepo,
        ITenantRepository tenantRepo)
    {
        _assignmentRepo = assignmentRepo;
        _tenantRepo = tenantRepo;
    }

    /// <inheritdoc />
    public async Task<UserProfileResponse?> GetCurrentUserProfileAsync(
        string userId,
        string? preferredActiveTenantId,
        string? displayNameFromClaims,
        string? emailFromClaims,
        CancellationToken cancellationToken = default)
    {
        var assignments = new List<UserRoleAssignment>();
        await foreach (var a in _assignmentRepo.GetAllByUserAsync(userId, cancellationToken).ConfigureAwait(false))
        {
            assignments.Add(a);
        }

        var active = assignments
            .Where(a => a.Status == UserStatus.Active)
            .OrderBy(a => a.TenantId, StringComparer.Ordinal)
            .ToList();

        if (active.Count == 0)
        {
            return null;
        }

        var distinctTenantIds = active.Select(a => a.TenantId).Distinct().ToList();
        var loaded = await Task.WhenAll(
            distinctTenantIds.Select(async tid =>
            {
                var t = await _tenantRepo.GetByIdAsync(tid, cancellationToken).ConfigureAwait(false);
                return (tid, t);
            })).ConfigureAwait(false);
        var tenantMap = loaded.ToDictionary(x => x.tid, x => x.t, StringComparer.Ordinal);

        var tenants = new List<TenantMembershipDto>(active.Count);
        foreach (var a in active)
        {
            tenantMap.TryGetValue(a.TenantId, out var tenantDoc);
            var tenantName = tenantDoc?.DisplayName is { Length: > 0 } dn
                ? dn
                : (tenantDoc?.Name ?? a.TenantId);
            tenants.Add(new TenantMembershipDto(a.TenantId, tenantName, a.Role.ToString()));
        }

        var preferred = preferredActiveTenantId?.Trim();
        UserRoleAssignment? activeAssignment;
        if (!string.IsNullOrEmpty(preferred))
        {
            activeAssignment = active.FirstOrDefault(a => string.Equals(a.TenantId, preferred, StringComparison.Ordinal));
            if (activeAssignment is null)
            {
                activeAssignment = active[0];
            }
        }
        else
        {
            activeAssignment = active[0];
        }

        var displayName = FirstNonEmpty(active.Select(a => a.DisplayName)) ?? displayNameFromClaims;
        var email = FirstNonEmpty(active.Select(a => a.Email)) ?? emailFromClaims;

        return new UserProfileResponse(
            Id: userId,
            UserId: userId,
            DisplayName: displayName,
            Email: email,
            ActiveTenant: activeAssignment.TenantId,
            Role: activeAssignment.Role.ToString(),
            Tenants: tenants);
    }

    private static string? FirstNonEmpty(IEnumerable<string?> values)
    {
        foreach (var v in values)
        {
            if (!string.IsNullOrWhiteSpace(v))
            {
                return v.Trim();
            }
        }

        return null;
    }
}

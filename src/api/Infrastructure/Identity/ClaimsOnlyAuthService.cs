using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;

namespace Todo.Api.Infrastructure.Identity;

/// <summary>
/// WO-8: when Cosmos repositories are not registered, returns a claims-only profile so local dev still gets 200 from GET /api/v1/auth/me.
/// </summary>
public sealed class ClaimsOnlyAuthService : IAuthService
{
    public Task<UserProfileResponse?> GetCurrentUserProfileAsync(
        string userId,
        string? preferredActiveTenantId,
        string? displayNameFromClaims,
        string? emailFromClaims,
        CancellationToken cancellationToken = default)
    {
        var profile = new UserProfileResponse(
            Id: userId,
            UserId: userId,
            DisplayName: displayNameFromClaims,
            Email: emailFromClaims,
            ActiveTenant: null,
            Role: null,
            Tenants: Array.Empty<TenantMembershipDto>());
        return Task.FromResult<UserProfileResponse?>(profile);
    }
}

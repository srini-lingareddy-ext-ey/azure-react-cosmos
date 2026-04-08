using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

/// <summary>WO-8: builds the authenticated user profile for <c>GET /api/v1/auth/me</c>.</summary>
public interface IAuthService
{
    /// <summary>
    /// Loads role assignments and tenant names. Returns <see langword="null"/> if the user has no active assignments (caller should return 403 USER_NOT_PROVISIONED).
    /// When Cosmos is not configured, implementations may return a claims-only profile without tenant data.
    /// </summary>
    Task<UserProfileResponse?> GetCurrentUserProfileAsync(
        string userId,
        string? preferredActiveTenantId,
        string? displayNameFromClaims,
        string? emailFromClaims,
        CancellationToken cancellationToken = default);
}

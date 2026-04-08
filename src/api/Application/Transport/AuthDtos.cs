using System.Text.Json.Serialization;

namespace Todo.Api.Application.Transport;

/// <summary>WO-8: GET /api/v1/auth/me response body.
/// <c>id</c> mirrors <c>userId</c> (Entra OID) so the object can be used as a generic resource with a stable identifier.</summary>
public sealed record UserProfileResponse(
    /// <summary>Alias for <see cref="UserId"/>; provides a consistent <c>id</c> field for frontend resource conventions.</summary>
    [property: JsonPropertyName("id")] string Id,
    /// <summary>Entra Object ID (oid claim) of the authenticated user.</summary>
    [property: JsonPropertyName("userId")] string UserId,
    [property: JsonPropertyName("displayName")] string? DisplayName,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("activeTenant")] string? ActiveTenant,
    [property: JsonPropertyName("role")] string? Role,
    [property: JsonPropertyName("tenants")] IReadOnlyList<TenantMembershipDto> Tenants);

/// <summary>One tenant membership for the current user.</summary>
public sealed record TenantMembershipDto(
    [property: JsonPropertyName("tenantId")] string TenantId,
    [property: JsonPropertyName("tenantName")] string TenantName,
    [property: JsonPropertyName("role")] string Role);

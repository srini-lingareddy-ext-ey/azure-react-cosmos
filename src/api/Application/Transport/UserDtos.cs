using System.Text.Json.Serialization;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Transport;

/// <summary>WO-10: single user row in a tenant roster.</summary>
public sealed class UserResponse
{
    public string UserId { get; set; } = string.Empty;

    public string? DisplayName { get; set; }

    public string? Email { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole Role { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserStatus Status { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }
}

/// <summary>WO-10: paginated roster (<c>limit</c>/<c>offset</c>).</summary>
public sealed class UserRosterResponse
{
    public IReadOnlyList<UserResponse> Items { get; set; } = Array.Empty<UserResponse>();

    public int TotalCount { get; set; }

    public int Limit { get; set; }

    public int Offset { get; set; }
}

/// <summary>WO-10: <c>PATCH .../users/{userId}/role</c></summary>
public sealed class ChangeRoleRequest
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole Role { get; set; }
}

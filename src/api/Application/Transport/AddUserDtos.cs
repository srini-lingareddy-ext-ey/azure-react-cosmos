using System.Text.Json.Serialization;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Transport;

/// <summary>WO-11: <c>POST /api/v1/tenants/{tenantId}/users</c></summary>
public sealed class AddUserRequest
{
    public string? UserId { get; set; }

    public string? Email { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole Role { get; set; }
}

/// <summary>WO-11: 201 response for add user / invitation.</summary>
public sealed class AddUserResponse
{
    /// <summary><c>assignment</c> or <c>invitation</c>.</summary>
    public string Kind { get; set; } = string.Empty;

    public string Id { get; set; } = string.Empty;
}

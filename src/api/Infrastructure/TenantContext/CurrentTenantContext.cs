using Todo.Api.Application.Services;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Infrastructure.TenantContext;

/// <summary>
/// Scoped store for resolved tenant and role. <see cref="IsSet"/> is false until middleware succeeds.
/// </summary>
public sealed class CurrentTenantContext : ICurrentTenantContext
{
    public bool IsSet { get; private set; }

    public string TenantId { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public UserStatus Status { get; private set; }

    internal void Set(string tenantId, UserRole role, UserStatus status)
    {
        TenantId = tenantId;
        Role = role;
        Status = status;
        IsSet = true;
    }
}

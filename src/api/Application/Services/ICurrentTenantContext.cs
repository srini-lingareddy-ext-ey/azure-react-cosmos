using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Services;

/// <summary>
/// Per-request application tenant and role after <c>X-Tenant-Id</c> resolution (WO-6).
/// Populated by tenant context middleware for endpoints marked with <c>RequireTenantContext</c>.
/// </summary>
public interface ICurrentTenantContext
{
    /// <summary>True after middleware validated tenant and user assignment for this request.</summary>
    bool IsSet { get; }

    /// <summary>Application tenant id (Cosmos <c>tenant</c> id). Meaningful only when <see cref="IsSet"/> is true.</summary>
    string TenantId { get; }

    UserRole Role { get; }

    UserStatus Status { get; }
}

using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Services;

/// <summary>
/// Per-request application tenant, role, and assignment after <c>X-Tenant-Id</c> resolution (WO-6, WO-7).
/// Populated by tenant context middleware for endpoints marked with <c>RequireTenantContext</c>.
/// </summary>
public interface ICurrentTenantContext : ITenantContext
{
    /// <summary>True after middleware validated tenant and user access for this request.</summary>
    bool IsSet { get; }

    /// <summary>Application role for the user in this tenant.</summary>
    UserRole Role { get; }

    /// <summary>User assignment status in this tenant.</summary>
    UserStatus AssignmentStatus { get; }
}

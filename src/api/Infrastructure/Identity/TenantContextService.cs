using Todo.Api.Application.Services;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Infrastructure.Identity;

/// <summary>
/// Scoped request tenant context (WO-7). Implements <see cref="ITenantContext"/> and <see cref="ICurrentTenantContext"/>.
/// </summary>
public sealed class TenantContextService : ICurrentTenantContext
{
    public bool IsSet { get; private set; }

    public string TenantId { get; private set; } = string.Empty;

    public string TenantName { get; private set; } = string.Empty;

    public TenantStatus Status { get; private set; }

    public UserRole Role { get; private set; }

    public UserStatus AssignmentStatus { get; private set; }

    internal void Set(
        string tenantId,
        string tenantName,
        TenantStatus tenantStatus,
        UserRole role,
        UserStatus assignmentStatus)
    {
        TenantId = tenantId;
        TenantName = tenantName;
        Status = tenantStatus;
        Role = role;
        AssignmentStatus = assignmentStatus;
        IsSet = true;
    }
}

using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Services;

/// <summary>
/// Resolved application tenant for the current request (WO-7). Populated by tenant context middleware when validation succeeds.
/// </summary>
public interface ITenantContext
{
    string TenantId { get; }

    string TenantName { get; }

    /// <summary>Tenant lifecycle status from the tenant aggregate.</summary>
    TenantStatus Status { get; }
}

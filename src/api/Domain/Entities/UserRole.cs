namespace Todo.Api.Domain.Entities;

/// <summary>Application role for a user within a tenant (WO-5).</summary>
public enum UserRole
{
    Viewer = 0,
    Operator = 1,
    Admin = 2,
    ComplianceOfficer = 3,
    PlatformAdmin = 4,
}

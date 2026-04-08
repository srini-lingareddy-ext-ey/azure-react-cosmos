namespace Todo.Api.Domain.Entities;

/// <summary>Activation status for a user role assignment (WO-5). Distinct from <see cref="TenantStatus"/>.</summary>
public enum UserStatus
{
    Active = 0,
    Inactive = 1,
}

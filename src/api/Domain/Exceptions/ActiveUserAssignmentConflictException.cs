namespace Todo.Api.Domain.Exceptions;

/// <summary>WO-11: user already has an active role assignment in the tenant.</summary>
public sealed class ActiveUserAssignmentConflictException : Exception
{
    public ActiveUserAssignmentConflictException(string userId, string tenantId)
        : base($"User '{userId}' already has an active role assignment in tenant '{tenantId}'.")
    {
        UserId = userId;
        TenantId = tenantId;
    }

    public string UserId { get; }

    public string TenantId { get; }
}

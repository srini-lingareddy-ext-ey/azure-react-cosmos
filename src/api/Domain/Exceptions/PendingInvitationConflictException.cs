namespace Todo.Api.Domain.Exceptions;

/// <summary>WO-11: a pending invitation already exists for this email in the tenant.</summary>
public sealed class PendingInvitationConflictException : Exception
{
    public PendingInvitationConflictException(string email, string tenantId)
        : base($"A pending invitation already exists for '{email}' in tenant '{tenantId}'.")
    {
        Email = email;
        TenantId = tenantId;
    }

    public string Email { get; }

    public string TenantId { get; }
}

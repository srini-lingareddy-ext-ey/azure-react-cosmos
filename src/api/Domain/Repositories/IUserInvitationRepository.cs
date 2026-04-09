using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

/// <summary>WO-11: persistence for <see cref="UserInvitation"/>.</summary>
public interface IUserInvitationRepository
{
    Task<UserInvitation> CreateAsync(UserInvitation invitation, CancellationToken cancellationToken = default);

    Task<UserInvitation?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);

    /// <summary>Latest matching invitation for email in tenant (for duplicate checks).</summary>
    Task<UserInvitation?> GetByEmailAndTenantAsync(string email, string tenantId, CancellationToken cancellationToken = default);
}

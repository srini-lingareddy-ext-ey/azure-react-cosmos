using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>WO-11: Cosmos <c>user-invitation</c>, partition <c>/tenantId</c>.</summary>
public sealed class UserInvitationRepository : IUserInvitationRepository
{
    private readonly IRepository<UserInvitation> _repository;

    public UserInvitationRepository(IRepository<UserInvitation> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<UserInvitation> CreateAsync(UserInvitation invitation, CancellationToken cancellationToken = default)
    {
        if (invitation.SchemaVersion == 0)
        {
            invitation.SchemaVersion = 1;
        }

        return await _repository.CreateAsync(invitation, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<UserInvitation?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, tenantId, cancellationToken);

    /// <inheritdoc />
    public async Task<UserInvitation?> GetByEmailAndTenantAsync(
        string email,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var normalized = email.Trim();
        var spec = new QuerySpec(
            "SELECT * FROM c WHERE c.tenantId = @tenantId AND c.email = @email ORDER BY c.invitedAt DESC",
            new Dictionary<string, object>
            {
                ["@tenantId"] = tenantId,
                ["@email"] = normalized,
            });
        await foreach (var row in _repository.QueryAsync(spec, cancellationToken).ConfigureAwait(false))
        {
            return row;
        }

        return null;
    }
}

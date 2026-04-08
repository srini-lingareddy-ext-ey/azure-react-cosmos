using Todo.Api.Domain.Entities;

namespace Todo.Api.Tests.Builders;

/// <summary>Test data builder for <see cref="UserRoleAssignment"/> (WO-5).</summary>
public sealed class UserRoleAssignmentBuilder
{
    private string _id = "ura-1";
    private string _tenantId = "t-1";
    private string _userId = "user-1";

    public UserRoleAssignmentBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public UserRoleAssignmentBuilder WithTenantId(string tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public UserRoleAssignmentBuilder WithUserId(string userId)
    {
        _userId = userId;
        return this;
    }

    public UserRoleAssignment Build() =>
        new()
        {
            Id = _id,
            TenantId = _tenantId,
            UserId = _userId,
            Email = "a@b.com",
            DisplayName = "Test User",
            Role = UserRole.Viewer,
            Status = UserStatus.Active,
            SchemaVersion = 1,
        };
}

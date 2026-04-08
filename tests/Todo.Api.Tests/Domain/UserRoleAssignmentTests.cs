using Todo.Api.Domain.Entities;
using Todo.Api.Tests.Builders;
using Xunit;

namespace Todo.Api.Tests.Domain;

public sealed class UserRoleAssignmentTests
{
    [Fact]
    public void UserRoleAssignment_PartitionKeyValue_Is_TenantId()
    {
        var a = new UserRoleAssignmentBuilder()
            .WithTenantId("tenant-x")
            .WithUserId("u1")
            .Build();
        Assert.Equal("tenant-x", a.TenantId);
        Assert.Equal("tenant-x", a.PartitionKeyValue);
    }

    [Fact]
    public void UserRoleAssignment_Implements_IDomainEntity_and_Audit_and_Concurrency()
    {
        var a = new UserRoleAssignmentBuilder().Build();
        Assert.True(a is IDomainEntity);
        Assert.True(a is IAuditableEntity);
        Assert.True(a is IConcurrencyEntity);
    }

    [Fact]
    public void UserRole_enum_has_required_values()
    {
        Assert.Equal(5, Enum.GetValues<UserRole>().Length);
        Assert.Contains(UserRole.PlatformAdmin, Enum.GetValues<UserRole>());
    }

    [Fact]
    public void UserStatus_enum_has_Active_and_Inactive()
    {
        Assert.Equal(2, Enum.GetValues<UserStatus>().Length);
    }
}

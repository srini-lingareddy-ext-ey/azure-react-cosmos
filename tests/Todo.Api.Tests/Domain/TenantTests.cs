using Todo.Api.Domain.Entities;
using Todo.Api.Tests.Builders;
using Xunit;

namespace Todo.Api.Tests.Domain;

public sealed class TenantTests
{
    [Fact]
    public void Tenant_Implements_IDomainEntity()
    {
        var tenant = new TenantBuilder().WithId("x").Build();
        Assert.Equal("x", tenant.Id);
        Assert.Equal("x", tenant.PartitionKeyValue);
    }

    [Fact]
    public void Tenant_Implements_IAuditableEntity_and_IConcurrencyEntity()
    {
        var tenant = new TenantBuilder().Build();
        Assert.True(tenant is IAuditableEntity);
        Assert.True(tenant is IConcurrencyEntity);
    }

    [Fact]
    public void Tenant_PartitionKeyValue_Matches_Id()
    {
        var tenant = new TenantBuilder().WithId("pk-test").Build();
        Assert.Equal("pk-test", tenant.PartitionKeyValue);
    }

    [Fact]
    public void TenantStatus_Has_Active_and_Inactive()
    {
        Assert.Equal(0, (int)TenantStatus.Active);
        Assert.Equal(1, (int)TenantStatus.Inactive);
    }
}

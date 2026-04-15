using Todo.Api.Domain.Entities;
using Xunit;

namespace Todo.Api.Tests.Domain;

public sealed class WO76EntityTests
{
    [Fact]
    public void HealthScore_Implements_IDomainEntity_And_IConcurrencyEntity()
    {
        var entity = new HealthScore { TenantId = "tenant1" };
        Assert.True(entity is IDomainEntity);
        Assert.True(entity is IConcurrencyEntity);
        Assert.Equal("tenant1", entity.PartitionKeyValue);
    }

    [Fact]
    public void HealthScore_Defaults()
    {
        var entity = new HealthScore();
        Assert.Equal(1, entity.SchemaVersion);
        Assert.NotNull(entity.Dimensions);
        Assert.Empty(entity.Dimensions);
        Assert.False(entity.IsStale);
        Assert.Equal(HealthScoreStatus.Green, entity.Status);
    }

    [Fact]
    public void HealthScoreStatus_Enum_Values()
    {
        Assert.Equal(0, (int)HealthScoreStatus.Green);
        Assert.Equal(1, (int)HealthScoreStatus.Yellow);
        Assert.Equal(2, (int)HealthScoreStatus.Red);
    }

    [Fact]
    public void HealthDimension_Defaults()
    {
        var dim = new HealthDimension();
        Assert.Equal(string.Empty, dim.Key);
        Assert.Equal(string.Empty, dim.Label);
        Assert.Equal(0, dim.Score);
        Assert.Equal(0, dim.Weight);
        Assert.False(dim.IsActive);
    }

    [Fact]
    public void DimensionSnapshot_Implements_IDomainEntity()
    {
        var entity = new DimensionSnapshot { TenantId = "tenant2" };
        Assert.True(entity is IDomainEntity);
        Assert.False(entity is IConcurrencyEntity);
        Assert.Equal("tenant2", entity.PartitionKeyValue);
    }

    [Fact]
    public void DimensionSnapshot_Defaults()
    {
        var entity = new DimensionSnapshot();
        Assert.Equal(1, entity.SchemaVersion);
        Assert.False(string.IsNullOrEmpty(entity.Id));
        Assert.Equal(string.Empty, entity.DimensionKey);
    }

    [Fact]
    public void DashboardSummary_Implements_IDomainEntity_And_IConcurrencyEntity()
    {
        var entity = new DashboardSummary { TenantId = "tenant3" };
        Assert.True(entity is IDomainEntity);
        Assert.True(entity is IConcurrencyEntity);
        Assert.Equal("tenant3", entity.PartitionKeyValue);
    }

    [Fact]
    public void DashboardSummary_Defaults()
    {
        var entity = new DashboardSummary();
        Assert.Equal(1, entity.SchemaVersion);
        Assert.Equal(0, entity.ActiveIncidents);
        Assert.Equal(0, entity.ActiveIncidentsPrior);
        Assert.Equal(0.0, entity.SlaComplianceRate);
        Assert.Equal(0.0, entity.SlaComplianceRatePrior);
        Assert.Equal(0, entity.OpenCriticalAlerts);
        Assert.Equal(0, entity.OpenCriticalAlertsPrior);
        Assert.Equal(0.0, entity.DataQualityPassRate);
        Assert.Equal(0.0, entity.DataQualityPassRatePrior);
        Assert.Equal(0, entity.DegradedInfraComponents);
        Assert.Equal(0, entity.DegradedInfraComponentsPrior);
    }
}

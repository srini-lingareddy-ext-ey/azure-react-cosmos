using Todo.Api.Domain.Entities;
using Xunit;

namespace Todo.Api.Tests.Domain;

public sealed class WO77EntityTests
{
    [Fact]
    public void LineageNode_Implements_IDomainEntity()
    {
        var entity = new LineageNode { TenantId = "tenant1" };
        Assert.True(entity is IDomainEntity);
        Assert.False(entity is IConcurrencyEntity);
        Assert.Equal("tenant1", entity.PartitionKeyValue);
    }

    [Fact]
    public void LineageNode_Defaults()
    {
        var entity = new LineageNode();
        Assert.Equal(1, entity.SchemaVersion);
        Assert.Equal(string.Empty, entity.Id);
        Assert.Equal(string.Empty, entity.NodeId);
        Assert.Equal(string.Empty, entity.NodeName);
        Assert.NotNull(entity.UpstreamIds);
        Assert.Empty(entity.UpstreamIds);
        Assert.NotNull(entity.DownstreamIds);
        Assert.Empty(entity.DownstreamIds);
        Assert.Equal(LineageNodeType.Pipeline, entity.NodeType);
    }

    [Fact]
    public void LineageNodeType_Enum_Values()
    {
        Assert.Equal(0, (int)LineageNodeType.Pipeline);
        Assert.Equal(1, (int)LineageNodeType.Dataset);
        Assert.Equal(2, (int)LineageNodeType.Product);
    }

    [Fact]
    public void ImpactAnalysisResult_Implements_IDomainEntity_And_IConcurrencyEntity()
    {
        var entity = new ImpactAnalysisResult { TenantId = "tenant2" };
        Assert.True(entity is IDomainEntity);
        Assert.True(entity is IConcurrencyEntity);
        Assert.Equal("tenant2", entity.PartitionKeyValue);
    }

    [Fact]
    public void ImpactAnalysisResult_Defaults()
    {
        var entity = new ImpactAnalysisResult();
        Assert.Equal(1, entity.SchemaVersion);
        Assert.False(string.IsNullOrEmpty(entity.Id));
        Assert.Equal(ImpactAnalysisStatus.Pending, entity.Status);
        Assert.NotNull(entity.Upstream);
        Assert.Empty(entity.Upstream);
        Assert.NotNull(entity.Downstream);
        Assert.Empty(entity.Downstream);
        Assert.False(entity.AdditionalUpstreamExist);
        Assert.False(entity.AdditionalDownstreamExist);
        Assert.Equal(0, entity.AffectedDownstreamCount);
    }

    [Fact]
    public void ImpactAnalysisStatus_Enum_Values()
    {
        Assert.Equal(0, (int)ImpactAnalysisStatus.Pending);
        Assert.Equal(1, (int)ImpactAnalysisStatus.Complete);
        Assert.Equal(2, (int)ImpactAnalysisStatus.Unavailable);
    }

    [Fact]
    public void ImpactNodeStatus_Enum_Values()
    {
        Assert.Equal(0, (int)ImpactNodeStatus.Healthy);
        Assert.Equal(1, (int)ImpactNodeStatus.AtRisk);
        Assert.Equal(2, (int)ImpactNodeStatus.Failed);
        Assert.Equal(3, (int)ImpactNodeStatus.Unknown);
    }

    [Fact]
    public void ImpactNode_Defaults()
    {
        var node = new ImpactNode();
        Assert.Equal(string.Empty, node.NodeId);
        Assert.Equal(string.Empty, node.NodeName);
        Assert.Equal(ImpactNodeStatus.Unknown, node.CurrentStatus);
        Assert.Equal(0, node.Depth);
    }

    [Fact]
    public void LineageRefreshStatus_Implements_IDomainEntity()
    {
        var entity = new LineageRefreshStatus { TenantId = "tenant3" };
        Assert.True(entity is IDomainEntity);
        Assert.False(entity is IConcurrencyEntity);
        Assert.Equal("tenant3", entity.PartitionKeyValue);
    }

    [Fact]
    public void LineageRefreshStatus_Defaults()
    {
        var entity = new LineageRefreshStatus();
        Assert.Equal(1, entity.SchemaVersion);
        Assert.Equal(string.Empty, entity.Id);
        Assert.Equal(RefreshStatus.Success, entity.LastRefreshStatus);
        Assert.Equal(0, entity.NodeCount);
        Assert.Null(entity.RefreshDurationSeconds);
        Assert.Null(entity.LastErrorMessage);
    }

    [Fact]
    public void RefreshStatus_Enum_Values()
    {
        Assert.Equal(0, (int)RefreshStatus.Success);
        Assert.Equal(1, (int)RefreshStatus.Failed);
    }
}

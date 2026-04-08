using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Exceptions;
using Todo.Api.Domain.Repositories;
using Xunit;

namespace Todo.Api.Tests.Application;

public sealed class TenantServiceTests
{
    private const string UserId = "user-1";

    private static async IAsyncEnumerable<UserRoleAssignment> YieldAsync(
        params UserRoleAssignment[] items)
    {
        foreach (var i in items)
        {
            yield return i;
        }

        await Task.CompletedTask;
    }

    private static TenantService CreateSut(
        Mock<ITenantRepository> tenantRepo,
        Mock<IUserRoleAssignmentRepository> assignmentRepo,
        Mock<IDistributedCache>? cache = null)
    {
        cache ??= new Mock<IDistributedCache>();
        cache
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return new TenantService(tenantRepo.Object, assignmentRepo.Object, cache.Object);
    }

    [Fact]
    public async Task ListTenantsAsync_NotPlatformAdmin_ThrowsUnauthorizedAccessException()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserId, It.IsAny<CancellationToken>()))
            .Returns(YieldAsync(new UserRoleAssignment
            {
                TenantId = "t1",
                UserId = UserId,
                Role = UserRole.Viewer,
                Status = UserStatus.Active,
            }));

        var sut = CreateSut(tenantRepo, assignmentRepo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.ListTenantsAsync(UserId, 1, 20));
    }

    [Fact]
    public async Task CreateTenantAsync_DuplicateName_ThrowsTenantNameConflictException()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo
            .Setup(r => r.GetByNameAsync("acme", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = "t1", Name = "acme", DisplayName = "Acme" });

        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserId, It.IsAny<CancellationToken>()))
            .Returns(YieldAsync(new UserRoleAssignment
            {
                TenantId = "x",
                UserId = UserId,
                Role = UserRole.PlatformAdmin,
                Status = UserStatus.Active,
            }));

        var sut = CreateSut(tenantRepo, assignmentRepo);

        await Assert.ThrowsAsync<TenantNameConflictException>(() =>
            sut.CreateTenantAsync(
                UserId,
                new CreateTenantRequest { Name = "acme", DisplayName = "Acme Corp" }));
    }

    [Fact]
    public async Task PatchTenantConfigAsync_WeightsSumNot100_ThrowsInvalidOperationException()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo
            .Setup(r => r.GetByIdAsync("t1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant
            {
                Id = "t1",
                Name = "n",
                DisplayName = "D",
                Status = TenantStatus.Active,
                Config = new TenantConfig
                {
                    HealthScoreWeights = new Dictionary<string, double> { ["composite"] = 100 },
                },
            });
        tenantRepo
            .Setup(r => r.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant t, CancellationToken _) => t);

        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserId, It.IsAny<CancellationToken>()))
            .Returns(YieldAsync(new UserRoleAssignment
            {
                TenantId = "t1",
                UserId = UserId,
                Role = UserRole.PlatformAdmin,
                Status = UserStatus.Active,
            }));

        var sut = CreateSut(tenantRepo, assignmentRepo);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.PatchTenantConfigAsync(
                UserId,
                "t1",
                new UpdateTenantConfigRequest
                {
                    HealthScoreWeights = new Dictionary<string, double> { ["a"] = 30, ["b"] = 50 },
                }));
    }

    [Fact]
    public async Task ActivateTenantAsync_SetsStatusActive()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        var existing = new Tenant
        {
            Id = "t1",
            Name = "n",
            DisplayName = "D",
            Status = TenantStatus.Inactive,
        };
        tenantRepo.Setup(r => r.GetByIdAsync("t1", It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        tenantRepo
            .Setup(r => r.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant t, CancellationToken _) => t);

        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserId, It.IsAny<CancellationToken>()))
            .Returns(YieldAsync(new UserRoleAssignment
            {
                TenantId = "x",
                UserId = UserId,
                Role = UserRole.PlatformAdmin,
                Status = UserStatus.Active,
            }));

        var sut = CreateSut(tenantRepo, assignmentRepo);

        var result = await sut.ActivateTenantAsync(UserId, "t1");

        Assert.Equal(TenantStatus.Active, result.Status);
        tenantRepo.Verify(
            r => r.UpdateAsync(It.Is<Tenant>(t => t.Status == TenantStatus.Active), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeactivateTenantAsync_SetsStatusInactive()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo.Setup(r => r.GetByIdAsync("t1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant
            {
                Id = "t1",
                Name = "n",
                DisplayName = "D",
                Status = TenantStatus.Active,
            });
        tenantRepo
            .Setup(r => r.UpdateAsync(It.IsAny<Tenant>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant t, CancellationToken _) => t);

        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserId, It.IsAny<CancellationToken>()))
            .Returns(YieldAsync(new UserRoleAssignment
            {
                TenantId = "x",
                UserId = UserId,
                Role = UserRole.PlatformAdmin,
                Status = UserStatus.Active,
            }));

        var sut = CreateSut(tenantRepo, assignmentRepo);

        var result = await sut.DeactivateTenantAsync(UserId, "t1");

        Assert.Equal(TenantStatus.Inactive, result.Status);
    }

    [Fact]
    public void ValidateMergedHealthScoreWeights_InvalidSum_Throws()
    {
        var config = new TenantConfig
        {
            HealthScoreWeights = new Dictionary<string, double> { ["a"] = 40, ["b"] = 50 },
        };

        Assert.Throws<InvalidOperationException>(() => TenantService.ValidateMergedHealthScoreWeights(config));
    }

    [Fact]
    public void ValidateMergedHealthScoreWeights_EmptyWeights_NoThrow()
    {
        var config = new TenantConfig { HealthScoreWeights = new Dictionary<string, double>() };
        TenantService.ValidateMergedHealthScoreWeights(config);
    }
}

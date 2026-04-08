using Moq;
using Todo.Api.Application.Services;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Xunit;

namespace Todo.Api.Tests.Application;

/// <summary>WO-8: <see cref="AuthService"/> profile resolution.</summary>
public sealed class AuthServiceTests
{
    private const string UserId = "oid-test-user";

    private static async IAsyncEnumerable<UserRoleAssignment> ToAsyncEnumerable(
        IReadOnlyList<UserRoleAssignment> items)
    {
        foreach (var i in items)
        {
            yield return i;
        }

        await Task.CompletedTask;
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_NoActiveAssignments_ReturnsNull()
    {
        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserId, It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(Array.Empty<UserRoleAssignment>()));

        var tenantRepo = new Mock<ITenantRepository>();
        var sut = new AuthService(assignmentRepo.Object, tenantRepo.Object);

        var result = await sut.GetCurrentUserProfileAsync(UserId, null, null, null);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_InactiveOnly_ReturnsNull()
    {
        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserId, It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(new List<UserRoleAssignment>
            {
                new()
                {
                    Id = "a1",
                    TenantId = "t1",
                    UserId = UserId,
                    Role = UserRole.Viewer,
                    Status = UserStatus.Inactive,
                },
            }));

        var tenantRepo = new Mock<ITenantRepository>();
        var sut = new AuthService(assignmentRepo.Object, tenantRepo.Object);

        var result = await sut.GetCurrentUserProfileAsync(UserId, null, null, null);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_MultipleTenants_SelectsFirstByTenantId_WhenNoHeader()
    {
        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserId, It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(new List<UserRoleAssignment>
            {
                new()
                {
                    Id = "a2",
                    TenantId = "zebra",
                    UserId = UserId,
                    Role = UserRole.Admin,
                    Status = UserStatus.Active,
                },
                new()
                {
                    Id = "a1",
                    TenantId = "alpha",
                    UserId = UserId,
                    Role = UserRole.Operator,
                    Status = UserStatus.Active,
                },
            }));

        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo.Setup(r => r.GetByIdAsync("alpha", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = "alpha", Name = "Alpha Co", DisplayName = "Alpha", Status = TenantStatus.Active });
        tenantRepo.Setup(r => r.GetByIdAsync("zebra", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = "zebra", Name = "Zebra Co", DisplayName = "Zebra", Status = TenantStatus.Active });

        var sut = new AuthService(assignmentRepo.Object, tenantRepo.Object);

        var result = await sut.GetCurrentUserProfileAsync(UserId, null, "from-claims", null);

        Assert.NotNull(result);
        Assert.Equal("alpha", result!.ActiveTenant);
        Assert.Equal("Operator", result.Role);
        Assert.Equal(2, result.Tenants.Count);
        Assert.Contains(result.Tenants, t => t.TenantId == "alpha" && t.Role == "Operator");
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_PreferredHeader_SelectsMatchingTenant()
    {
        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserId, It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(new List<UserRoleAssignment>
            {
                new()
                {
                    Id = "a1",
                    TenantId = "t1",
                    UserId = UserId,
                    Role = UserRole.Viewer,
                    Status = UserStatus.Active,
                },
                new()
                {
                    Id = "a2",
                    TenantId = "t2",
                    UserId = UserId,
                    Role = UserRole.Admin,
                    Status = UserStatus.Active,
                },
            }));

        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo.Setup(r => r.GetByIdAsync("t1", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = "t1", Name = "T1", DisplayName = "One", Status = TenantStatus.Active });
        tenantRepo.Setup(r => r.GetByIdAsync("t2", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = "t2", Name = "T2", DisplayName = "Two", Status = TenantStatus.Active });

        var sut = new AuthService(assignmentRepo.Object, tenantRepo.Object);

        var result = await sut.GetCurrentUserProfileAsync(UserId, "t2", null, null);

        Assert.NotNull(result);
        Assert.Equal("t2", result!.ActiveTenant);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_UnknownPreferredHeader_FallsBackToFirstActive()
    {
        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserId, It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(new List<UserRoleAssignment>
            {
                new()
                {
                    Id = "a1",
                    TenantId = "only",
                    UserId = UserId,
                    Role = UserRole.Viewer,
                    Status = UserStatus.Active,
                },
            }));

        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo.Setup(r => r.GetByIdAsync("only", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = "only", Name = "Only", DisplayName = "", Status = TenantStatus.Active });

        var sut = new AuthService(assignmentRepo.Object, tenantRepo.Object);

        var result = await sut.GetCurrentUserProfileAsync(UserId, "missing-tenant", null, null);

        Assert.NotNull(result);
        Assert.Equal("only", result!.ActiveTenant);
        Assert.Equal("Viewer", result.Role);
    }
}

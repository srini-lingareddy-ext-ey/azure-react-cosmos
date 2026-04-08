using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Todo.Api.Application.Services;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Xunit;

namespace Todo.Api.Tests.Application;

public sealed class UserManagementServiceTests
{
    private const string TenantId = "tenant-1";
    private const string AdminId = "admin-1";

    private static async IAsyncEnumerable<UserRoleAssignment> YieldAsync(params UserRoleAssignment[] items)
    {
        foreach (var i in items)
        {
            yield return i;
        }

        await Task.CompletedTask;
    }

    private static UserManagementService CreateSut(Mock<IUserRoleAssignmentRepository> assignmentRepo)
    {
        return new UserManagementService(assignmentRepo.Object, NullLogger<UserManagementService>.Instance);
    }

    [Fact]
    public async Task GetRosterAsync_Pagination_ReturnsSliceAndTotal()
    {
        var rows = Enumerable.Range(0, 5)
            .Select(i => new UserRoleAssignment
            {
                TenantId = TenantId,
                UserId = $"u{i}",
                DisplayName = $"User {i}",
                Role = UserRole.Viewer,
                Status = UserStatus.Active,
            })
            .ToArray();

        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        var adminAssignment = new UserRoleAssignment
        {
            TenantId = TenantId,
            UserId = AdminId,
            Role = UserRole.Admin,
            Status = UserStatus.Active,
        };
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(AdminId, It.IsAny<CancellationToken>()))
            .Returns(YieldAsync(adminAssignment));
        assignmentRepo
            .Setup(r => r.GetByUserAndTenantAsync(AdminId, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminAssignment);
        assignmentRepo
            .Setup(r => r.GetAllByTenantAsync(TenantId, It.IsAny<CancellationToken>()))
            .Returns(YieldAsync(rows));

        var sut = CreateSut(assignmentRepo);

        var result = await sut.GetRosterAsync(AdminId, TenantId, null, null, limit: 2, offset: 1);

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Limit);
        Assert.Equal(1, result.Offset);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("u1", result.Items[0].UserId);
        Assert.Equal("u2", result.Items[1].UserId);
    }

    [Fact]
    public async Task ChangeUserRoleAsync_SelfRoleChange_ThrowsUnauthorizedAccessException()
    {
        var adminAssignment = new UserRoleAssignment
        {
            TenantId = TenantId,
            UserId = AdminId,
            Role = UserRole.Admin,
            Status = UserStatus.Active,
        };
        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(AdminId, It.IsAny<CancellationToken>()))
            .Returns(YieldAsync(adminAssignment));
        assignmentRepo
            .Setup(r => r.GetByUserAndTenantAsync(AdminId, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminAssignment);

        var sut = CreateSut(assignmentRepo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.ChangeUserRoleAsync(AdminId, TenantId, AdminId, UserRole.Operator));
    }

    [Fact]
    public async Task DeactivateUserAsync_UserMissing_ThrowsKeyNotFoundException()
    {
        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        var adminAssignment = new UserRoleAssignment
        {
            TenantId = TenantId,
            UserId = AdminId,
            Role = UserRole.Admin,
            Status = UserStatus.Active,
        };
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(AdminId, It.IsAny<CancellationToken>()))
            .Returns(YieldAsync(adminAssignment));
        assignmentRepo
            .Setup(r => r.GetByUserAndTenantAsync(AdminId, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(adminAssignment);
        assignmentRepo
            .Setup(r => r.GetByUserAndTenantAsync("missing", TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRoleAssignment?)null);

        var sut = CreateSut(assignmentRepo);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            sut.DeactivateUserAsync(AdminId, TenantId, "missing"));
    }

    [Fact]
    public async Task GetRosterAsync_ViewerRole_ThrowsUnauthorizedAccessException()
    {
        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync("viewer", It.IsAny<CancellationToken>()))
            .Returns(YieldAsync(new UserRoleAssignment
            {
                TenantId = TenantId,
                UserId = "viewer",
                Role = UserRole.Viewer,
                Status = UserStatus.Active,
            }));

        var sut = CreateSut(assignmentRepo);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            sut.GetRosterAsync("viewer", TenantId, null, null, 50, 0));
    }
}

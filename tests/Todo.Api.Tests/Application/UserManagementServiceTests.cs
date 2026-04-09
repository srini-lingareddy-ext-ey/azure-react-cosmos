using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Exceptions;
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

    private static UserManagementService CreateSut(
        Mock<IUserRoleAssignmentRepository> assignmentRepo,
        Mock<IUserInvitationRepository>? invitationRepo = null)
    {
        invitationRepo ??= new Mock<IUserInvitationRepository>();
        return new UserManagementService(
            assignmentRepo.Object,
            invitationRepo.Object,
            NullLogger<UserManagementService>.Instance);
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

    [Fact]
    public async Task AddUserAsync_ByUserId_CreatesAssignment_WhenNoneExists()
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
        assignmentRepo
            .Setup(r => r.GetByUserAndTenantAsync("new-user", TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRoleAssignment?)null);
        assignmentRepo
            .Setup(r => r.CreateAsync(It.IsAny<UserRoleAssignment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRoleAssignment a, CancellationToken _) => a);

        var sut = CreateSut(assignmentRepo);

        var result = await sut.AddUserAsync(
            AdminId,
            TenantId,
            new AddUserRequest { UserId = "new-user", Role = UserRole.Viewer });

        Assert.Equal("assignment", result.Kind);
        Assert.False(string.IsNullOrEmpty(result.Id));
        assignmentRepo.Verify(
            r => r.CreateAsync(It.Is<UserRoleAssignment>(a => a.UserId == "new-user" && a.Status == UserStatus.Active), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task AddUserAsync_ByEmail_CreatesInvitation_WithExpiry72h()
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
        assignmentRepo
            .Setup(r => r.GetAllByTenantAsync(TenantId, It.IsAny<CancellationToken>()))
            .Returns(YieldAsync());

        UserInvitation? created = null;
        var invitationRepo = new Mock<IUserInvitationRepository>();
        invitationRepo
            .Setup(r => r.GetByEmailAndTenantAsync("a@b.com", TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInvitation?)null);
        invitationRepo
            .Setup(r => r.CreateAsync(It.IsAny<UserInvitation>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserInvitation i, CancellationToken _) =>
            {
                created = i;
                return i;
            });

        var sut = CreateSut(assignmentRepo, invitationRepo);

        var result = await sut.AddUserAsync(
            AdminId,
            TenantId,
            new AddUserRequest { Email = "a@b.com", Role = UserRole.Operator });

        Assert.Equal("invitation", result.Kind);
        Assert.NotNull(created);
        Assert.Equal("a@b.com", created!.Email);
        Assert.Equal(InvitationStatus.Pending, created.Status);
        Assert.Equal(2592000, created.Ttl);
        var delta = created.ExpiresAt - created.InvitedAt;
        Assert.InRange(delta.TotalHours, 72 - 0.01, 72 + 0.01);
    }

    [Fact]
    public async Task AddUserAsync_DuplicateActiveAssignment_ThrowsActiveUserAssignmentConflictException()
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
        assignmentRepo
            .Setup(r => r.GetByUserAndTenantAsync("dup", TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserRoleAssignment
            {
                TenantId = TenantId,
                UserId = "dup",
                Status = UserStatus.Active,
                Role = UserRole.Viewer,
            });

        var sut = CreateSut(assignmentRepo);

        await Assert.ThrowsAsync<ActiveUserAssignmentConflictException>(() =>
            sut.AddUserAsync(AdminId, TenantId, new AddUserRequest { UserId = "dup", Role = UserRole.Admin }));
    }
}

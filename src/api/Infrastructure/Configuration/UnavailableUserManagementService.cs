using System.Net.Http;
using Todo.Api.Application.Services;
using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Infrastructure.Configuration;

/// <summary>WO-10: placeholder when Cosmos is not configured.</summary>
public sealed class UnavailableUserManagementService : IUserManagementService
{
    private static readonly HttpRequestException Ex = new("User management requires Azure Cosmos DB (AZURE_COSMOS_ENDPOINT) to be configured.");

    public Task<UserRosterResponse> GetRosterAsync(
        string actorUserId,
        string tenantId,
        UserRole? roleFilter,
        UserStatus? statusFilter,
        int limit,
        int offset,
        CancellationToken cancellationToken = default) =>
        throw Ex;

    public Task ChangeUserRoleAsync(
        string actorUserId,
        string tenantId,
        string targetUserId,
        UserRole newRole,
        CancellationToken cancellationToken = default) =>
        throw Ex;

    public Task DeactivateUserAsync(
        string actorUserId,
        string tenantId,
        string targetUserId,
        CancellationToken cancellationToken = default) =>
        throw Ex;

    public Task ActivateUserAsync(
        string actorUserId,
        string tenantId,
        string targetUserId,
        CancellationToken cancellationToken = default) =>
        throw Ex;

    public Task<AddUserResponse> AddUserAsync(
        string actorUserId,
        string tenantId,
        AddUserRequest request,
        CancellationToken cancellationToken = default) =>
        throw Ex;
}

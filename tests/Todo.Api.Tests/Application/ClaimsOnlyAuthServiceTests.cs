using Todo.Api.Infrastructure.Identity;
using Xunit;

namespace Todo.Api.Tests.Application;

/// <summary>WO-8: <see cref="ClaimsOnlyAuthService"/> always returns a non-null claims-only profile.</summary>
public sealed class ClaimsOnlyAuthServiceTests
{
    private readonly ClaimsOnlyAuthService _sut = new();

    [Fact]
    public async Task GetCurrentUserProfileAsync_AlwaysReturnsNonNull()
    {
        var result = await _sut.GetCurrentUserProfileAsync("oid-123", null, "Alice", "alice@example.com");

        Assert.NotNull(result);
        Assert.Equal("oid-123", result!.Id);
        Assert.Equal("oid-123", result.UserId);
        Assert.Equal("Alice", result.DisplayName);
        Assert.Equal("alice@example.com", result.Email);
        Assert.Null(result.ActiveTenant);
        Assert.Null(result.Role);
        Assert.Empty(result.Tenants);
    }

    [Fact]
    public async Task GetCurrentUserProfileAsync_NullClaims_ReturnsProfileWithNulls()
    {
        var result = await _sut.GetCurrentUserProfileAsync("oid-456", "preferred-tenant", null, null);

        Assert.NotNull(result);
        Assert.Equal("oid-456", result!.UserId);
        Assert.Null(result.DisplayName);
        Assert.Null(result.Email);
        Assert.Empty(result.Tenants);
    }
}

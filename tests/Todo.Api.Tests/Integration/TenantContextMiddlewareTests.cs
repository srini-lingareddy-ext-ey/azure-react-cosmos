using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Xunit;

namespace Todo.Api.Tests.Integration;

/// <summary>WO-6 / WO-7: X-Tenant-Id, tenant/assignment checks, error codes, PlatformAdmin bypass.</summary>
[Trait(TestTraits.Category, TestTraits.FullCI)]
public sealed class TenantContextMiddlewareTests
{
    private const string TenantId = "tenant-integration-1";
    private const string UserOid = "user-oid-integration-1";

    private sealed class TenantContextWebAppFactory : WebApplicationFactory<global::Program>
    {
        private readonly IUserRoleAssignmentRepository _assignmentRepo;
        private readonly ITenantRepository _tenantRepo;

        public TenantContextWebAppFactory(
            IUserRoleAssignmentRepository assignmentRepo,
            ITenantRepository tenantRepo)
        {
            _assignmentRepo = assignmentRepo;
            _tenantRepo = tenantRepo;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["AZURE_COSMOS_ENDPOINT"] = "",
                    ["Authentication:JwtBearer:Authority"] = TenantContextIntegrationTestAuth.Issuer,
                    ["Authentication:JwtBearer:Audience"] = TenantContextIntegrationTestAuth.Audience,
                    ["Authentication:JwtBearer:RequireHttpsMetadata"] = "false",
                });
            });
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton(_assignmentRepo);
                services.AddSingleton(_tenantRepo);
                services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.MetadataAddress = null!;
                    o.ConfigurationManager = null!;
                    o.MapInboundClaims = false;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = TenantContextIntegrationTestAuth.Issuer,
                        ValidateAudience = true,
                        ValidAudience = TenantContextIntegrationTestAuth.Audience,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = TenantContextIntegrationTestAuth.SigningKey,
                        ClockSkew = TimeSpan.Zero,
                    };
                });
            });
        }
    }

    private static WebApplicationFactory<global::Program> CreateFactory(
        IUserRoleAssignmentRepository assignmentRepo,
        ITenantRepository tenantRepo) =>
        new TenantContextWebAppFactory(assignmentRepo, tenantRepo);

    private static HttpClient CreateAuthenticatedClient(WebApplicationFactory<global::Program> factory)
    {
        var client = factory.CreateClient();
        var jwt = TenantContextIntegrationTestAuth.CreateBearerToken(UserOid);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
        return client;
    }

    private static async IAsyncEnumerable<UserRoleAssignment> YieldAssignmentsAsync(
        params UserRoleAssignment[] items)
    {
        foreach (var i in items)
        {
            yield return i;
        }

        await Task.CompletedTask.ConfigureAwait(false);
    }

    [Fact]
    public async Task AuthMe_WithoutTenantHeader_Returns200()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        using var factory = CreateFactory(assignmentRepo.Object, tenantRepo.Object);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.GetAsync("/api/v1/auth/me");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(UserOid, body.GetProperty("id").GetString());
    }

    [Fact]
    public async Task TenantContext_MissingHeader_Returns403_TenantAccessDenied()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        using var factory = CreateFactory(assignmentRepo.Object, tenantRepo.Object);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.GetAsync("/api/v1/me/tenant-context");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("TENANT_ACCESS_DENIED", doc.GetProperty("errorCode").GetString());
        tenantRepo.Verify(
            r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TenantContext_NoAssignment_Returns403_TenantAccessDenied()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo
            .Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = TenantId, Name = "t", DisplayName = "T", Status = TenantStatus.Active });

        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetByUserAndTenantAsync(UserOid, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRoleAssignment?)null);
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserOid, It.IsAny<CancellationToken>()))
            .Returns(YieldAssignmentsAsync());

        using var factory = CreateFactory(assignmentRepo.Object, tenantRepo.Object);
        using var client = CreateAuthenticatedClient(factory);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantId);

        var response = await client.GetAsync("/api/v1/me/tenant-context");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("TENANT_ACCESS_DENIED", doc.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task TenantContext_InactiveAssignment_Returns403_TenantAccessDenied()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo
            .Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = TenantId, Name = "t", DisplayName = "T", Status = TenantStatus.Active });

        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetByUserAndTenantAsync(UserOid, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserRoleAssignment
            {
                Id = "a1",
                TenantId = TenantId,
                UserId = UserOid,
                Role = UserRole.Viewer,
                Status = UserStatus.Inactive,
            });
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserOid, It.IsAny<CancellationToken>()))
            .Returns(YieldAssignmentsAsync());

        using var factory = CreateFactory(assignmentRepo.Object, tenantRepo.Object);
        using var client = CreateAuthenticatedClient(factory);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantId);

        var response = await client.GetAsync("/api/v1/me/tenant-context");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("TENANT_ACCESS_DENIED", doc.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task TenantContext_InactiveTenant_Returns403_TenantInactive()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo
            .Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = TenantId, Name = "t", DisplayName = "T", Status = TenantStatus.Inactive });

        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserOid, It.IsAny<CancellationToken>()))
            .Returns(YieldAssignmentsAsync());

        using var factory = CreateFactory(assignmentRepo.Object, tenantRepo.Object);
        using var client = CreateAuthenticatedClient(factory);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantId);

        var response = await client.GetAsync("/api/v1/me/tenant-context");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("TENANT_INACTIVE", doc.GetProperty("errorCode").GetString());
        assignmentRepo.Verify(
            r => r.GetByUserAndTenantAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TenantContext_TenantNotFound_Returns403_TenantAccessDenied()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo
            .Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserOid, It.IsAny<CancellationToken>()))
            .Returns(YieldAssignmentsAsync());

        using var factory = CreateFactory(assignmentRepo.Object, tenantRepo.Object);
        using var client = CreateAuthenticatedClient(factory);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantId);

        var response = await client.GetAsync("/api/v1/me/tenant-context");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var doc = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("TENANT_ACCESS_DENIED", doc.GetProperty("errorCode").GetString());
        assignmentRepo.Verify(
            r => r.GetByUserAndTenantAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TenantContext_PlatformAdmin_WithoutAssignmentForTenant_Returns200_WithPlatformAdminRole()
    {
        const string otherTenant = "tenant-other";
        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo
            .Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = TenantId, Name = "t", DisplayName = "T", Status = TenantStatus.Active });

        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetByUserAndTenantAsync(UserOid, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRoleAssignment?)null);
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserOid, It.IsAny<CancellationToken>()))
            .Returns(YieldAssignmentsAsync(new UserRoleAssignment
            {
                Id = "pa1",
                TenantId = otherTenant,
                UserId = UserOid,
                Role = UserRole.PlatformAdmin,
                Status = UserStatus.Active,
            }));

        using var factory = CreateFactory(assignmentRepo.Object, tenantRepo.Object);
        using var client = CreateAuthenticatedClient(factory);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantId);

        var response = await client.GetAsync("/api/v1/me/tenant-context");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(TenantId, body.GetProperty("tenantId").GetString());
        Assert.Equal("PlatformAdmin", body.GetProperty("role").GetString());
        assignmentRepo.Verify(
            r => r.GetByUserAndTenantAsync(UserOid, TenantId, It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TenantContext_ValidAssignment_Returns200_WithRole()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo
            .Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = TenantId, Name = "t", DisplayName = "T", Status = TenantStatus.Active });

        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetByUserAndTenantAsync(UserOid, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserRoleAssignment
            {
                Id = "a1",
                TenantId = TenantId,
                UserId = UserOid,
                Role = UserRole.Operator,
                Status = UserStatus.Active,
            });
        assignmentRepo
            .Setup(r => r.GetAllByUserAsync(UserOid, It.IsAny<CancellationToken>()))
            .Returns(YieldAssignmentsAsync());

        using var factory = CreateFactory(assignmentRepo.Object, tenantRepo.Object);
        using var client = CreateAuthenticatedClient(factory);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantId);

        var response = await client.GetAsync("/api/v1/me/tenant-context");

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(TenantId, body.GetProperty("tenantId").GetString());
        Assert.Equal("Operator", body.GetProperty("role").GetString());
        Assert.Equal("Active", body.GetProperty("status").GetString());
    }
}

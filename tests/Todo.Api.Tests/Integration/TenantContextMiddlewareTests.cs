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

/// <summary>WO-6: X-Tenant-Id, tenant/assignment checks, and tenant context on /api/v1/me/tenant-context.</summary>
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
                    // Keep short claim names so ICurrentUserService finds "oid" (same as typical Entra v2 tokens).
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

    [Fact]
    public async Task TenantContext_MissingHeader_Returns400()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        using var factory = CreateFactory(assignmentRepo.Object, tenantRepo.Object);
        using var client = CreateAuthenticatedClient(factory);

        var response = await client.GetAsync("/api/v1/me/tenant-context");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        tenantRepo.Verify(
            r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TenantContext_NoAssignment_Returns403()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo
            .Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = TenantId, Name = "t", DisplayName = "T", Status = TenantStatus.Active });

        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();
        assignmentRepo
            .Setup(r => r.GetByUserAndTenantAsync(UserOid, TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRoleAssignment?)null);

        using var factory = CreateFactory(assignmentRepo.Object, tenantRepo.Object);
        using var client = CreateAuthenticatedClient(factory);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantId);

        var response = await client.GetAsync("/api/v1/me/tenant-context");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TenantContext_InactiveAssignment_Returns403()
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

        using var factory = CreateFactory(assignmentRepo.Object, tenantRepo.Object);
        using var client = CreateAuthenticatedClient(factory);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantId);

        var response = await client.GetAsync("/api/v1/me/tenant-context");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task TenantContext_InactiveTenant_Returns403()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo
            .Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Tenant { Id = TenantId, Name = "t", DisplayName = "T", Status = TenantStatus.Inactive });

        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();

        using var factory = CreateFactory(assignmentRepo.Object, tenantRepo.Object);
        using var client = CreateAuthenticatedClient(factory);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantId);

        var response = await client.GetAsync("/api/v1/me/tenant-context");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        assignmentRepo.Verify(
            r => r.GetByUserAndTenantAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task TenantContext_TenantNotFound_Returns404()
    {
        var tenantRepo = new Mock<ITenantRepository>();
        tenantRepo
            .Setup(r => r.GetByIdAsync(TenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Tenant?)null);

        var assignmentRepo = new Mock<IUserRoleAssignmentRepository>();

        using var factory = CreateFactory(assignmentRepo.Object, tenantRepo.Object);
        using var client = CreateAuthenticatedClient(factory);
        client.DefaultRequestHeaders.Add("X-Tenant-Id", TenantId);

        var response = await client.GetAsync("/api/v1/me/tenant-context");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        assignmentRepo.Verify(
            r => r.GetByUserAndTenantAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
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

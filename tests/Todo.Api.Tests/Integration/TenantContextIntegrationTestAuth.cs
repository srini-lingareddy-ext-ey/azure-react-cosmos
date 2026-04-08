using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Todo.Api.Tests.Integration;

/// <summary>Symmetric JWTs for integration tests (WO-6); PostConfigure JwtBearer to match issuer, audience, and key.</summary>
internal static class TenantContextIntegrationTestAuth
{
    public const string Issuer = "https://integration.tests.local/v2.0";
    public const string Audience = "api://integration-tests";

    internal static readonly SymmetricSecurityKey SigningKey =
        new(Encoding.UTF8.GetBytes("IntegrationTests_SigningKey_32chars_!!"));

    public static string CreateBearerToken(string oid)
    {
        var creds = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: [new Claim("oid", oid)],
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

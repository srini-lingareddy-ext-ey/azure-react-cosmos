namespace Todo.Api.Domain.Entities;

/// <summary>Optional branding for a tenant (WO-4). All fields nullable per blueprint.</summary>
public sealed class TenantBranding
{
    public string? LogoUrl { get; set; }
    public string? BackgroundImageUrl { get; set; }
}

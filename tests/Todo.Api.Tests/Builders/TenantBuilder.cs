using Todo.Api.Domain.Entities;

namespace Todo.Api.Tests.Builders;

/// <summary>Test data builder for <see cref="Tenant"/> (WO-4).</summary>
public sealed class TenantBuilder
{
    private string _id = "t-1";
    private string _name = "acme";
    private string _displayName = "Acme";
    private TenantStatus _status = TenantStatus.Active;
    private TenantBranding? _branding;
    private TenantConfig? _config;
    private DateTimeOffset? _createdAt;
    private DateTimeOffset? _updatedAt;
    private string? _createdBy;
    private string? _updatedBy;

    public TenantBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public TenantBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public TenantBuilder WithDisplayName(string displayName)
    {
        _displayName = displayName;
        return this;
    }

    public TenantBuilder WithStatus(TenantStatus status)
    {
        _status = status;
        return this;
    }

    public TenantBuilder WithBranding(TenantBranding? branding)
    {
        _branding = branding;
        return this;
    }

    public TenantBuilder WithConfig(TenantConfig? config)
    {
        _config = config;
        return this;
    }

    public TenantBuilder WithAuditFields(
        DateTimeOffset? createdAt,
        DateTimeOffset? updatedAt,
        string? createdBy,
        string? updatedBy)
    {
        _createdAt = createdAt;
        _updatedAt = updatedAt;
        _createdBy = createdBy;
        _updatedBy = updatedBy;
        return this;
    }

    public Tenant Build() =>
        new()
        {
            Id = _id,
            Name = _name,
            DisplayName = _displayName,
            Status = _status,
            Branding = _branding,
            Config = _config,
            CreatedAt = _createdAt,
            UpdatedAt = _updatedAt,
            CreatedBy = _createdBy,
            UpdatedBy = _updatedBy,
            SchemaVersion = 1,
        };
}

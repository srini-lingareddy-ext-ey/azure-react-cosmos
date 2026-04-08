using Todo.Api.Domain.Entities;

namespace Todo.Api.Tests.Builders;

/// <summary>Test data builder for <see cref="Tenant"/> (WO-4).</summary>
public sealed class TenantBuilder
{
    private string _id = "t-1";
    private string _name = "acme";
    private TenantStatus _status = TenantStatus.Active;

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

    public TenantBuilder WithStatus(TenantStatus status)
    {
        _status = status;
        return this;
    }

    public Tenant Build() =>
        new()
        {
            Id = _id,
            Name = _name,
            DisplayName = "Acme",
            Status = _status,
            SchemaVersion = 1,
        };
}

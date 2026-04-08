namespace Todo.Api.Domain.Exceptions;

/// <summary>Raised when creating a tenant whose <see cref="Entities.Tenant.Name"/> already exists (WO-9).</summary>
public sealed class TenantNameConflictException : Exception
{
    public TenantNameConflictException(string name)
        : base($"A tenant with name '{name}' already exists.")
    {
        Name = name;
    }

    public string Name { get; }
}

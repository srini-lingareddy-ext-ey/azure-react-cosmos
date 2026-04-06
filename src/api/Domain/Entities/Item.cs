namespace Todo.Api.Domain.Entities;

// =============================================================================
// Phase 1 marker — domain-specific entities (e.g. Tenant, UserRoleAssignment)
// will be added here by Phase 2 feature work orders. Do not introduce new
// concrete domain types in this file except as directed by those work orders.
// =============================================================================

/// <summary>
/// Minimal placeholder for the default Cosmos "Items" container (partition key /id).
/// Satisfies AC-FOUNDATION-002.7 end-to-end repository registration in Program.cs until
/// Phase 2 replaces this with real domain models.
/// </summary>
public sealed class Item : IDomainEntity, IAuditableEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public object PartitionKeyValue => Id;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Etag { get; set; }
}

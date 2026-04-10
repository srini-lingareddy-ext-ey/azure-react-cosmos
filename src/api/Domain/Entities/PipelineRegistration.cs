namespace Todo.Api.Domain.Entities;

/// <summary>Medallion layer classification for pipeline data flow (WO-15).</summary>
public enum MedallionLayer
{
    Raw = 0,
    Mirror = 1,
    Model = 2,
    Consumption = 3,
}

/// <summary>
/// Pipeline registration entity (WO-15). Container <c>pipeline-registration</c>, partition <c>/tenantId</c>.
/// </summary>
public sealed class PipelineRegistration : IDomainEntity, IAuditableEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string PipelineName { get; set; } = string.Empty;

    public string? SourceSystem { get; set; }

    public string? TargetSystem { get; set; }

    public MedallionLayer MedallionLayer { get; set; }

    public string? BusinessPlanId { get; set; }

    /// <summary>Denormalized from BusinessPlan.Name for read performance.</summary>
    public string? BusinessPlanName { get; set; }

    public string? Domain { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int SchemaVersion { get; set; } = 1;

    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
    public string? Etag { get; set; }

    /// <inheritdoc />
    public object PartitionKeyValue => TenantId;
}

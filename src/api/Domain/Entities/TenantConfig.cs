namespace Todo.Api.Domain.Entities;

/// <summary>Tenant-specific configuration (WO-4).</summary>
public sealed class TenantConfig
{
    /// <summary>Weights per dimension; must sum to 100 (validated in WO-8).</summary>
    public Dictionary<string, double> HealthScoreWeights { get; set; } = new();

    public HealthStatusThresholds? HealthStatusThresholds { get; set; }
}

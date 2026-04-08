namespace Todo.Api.Domain.Entities;

/// <summary>
/// Thresholds for mapping health scores to status bands (WO-4).
/// Validation that weights sum to 100 is enforced in the service layer (WO-8), not here.
/// </summary>
public sealed class HealthStatusThresholds
{
    /// <summary>Minimum composite score to treat as healthy (inclusive).</summary>
    public double? HealthyMin { get; set; }

    /// <summary>Minimum composite score to treat as warning (inclusive).</summary>
    public double? WarningMin { get; set; }

    /// <summary>Scores below this are critical (exclusive), if set.</summary>
    public double? CriticalBelow { get; set; }
}

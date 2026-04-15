namespace Todo.Api.Domain.Entities;

public sealed class DashboardSummary : IDomainEntity, IConcurrencyEntity
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public int ActiveIncidents { get; set; }
    public int ActiveIncidentsPrior { get; set; }
    public double SlaComplianceRate { get; set; }
    public double SlaComplianceRatePrior { get; set; }
    public int OpenCriticalAlerts { get; set; }
    public int OpenCriticalAlertsPrior { get; set; }
    public double DataQualityPassRate { get; set; }
    public double DataQualityPassRatePrior { get; set; }
    public int DegradedInfraComponents { get; set; }
    public int DegradedInfraComponentsPrior { get; set; }
    public DateTimeOffset? CalculatedAt { get; set; }
    public int SchemaVersion { get; set; } = 1;
    public string? Etag { get; set; }
    public object PartitionKeyValue => TenantId;
}

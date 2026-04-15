namespace Todo.Api.Application.Transport;

/// <summary>WO-83: Dashboard KPI and chart response DTOs.</summary>
public sealed class KpiValue
{
    public double? Value { get; set; }
    public string? Unit { get; set; }
    public string? TrendDirection { get; set; }
}

public sealed class DashboardKpisResponse
{
    public KpiValue ActiveIncidents { get; set; } = new();
    public KpiValue SlaComplianceRate { get; set; } = new();
    public KpiValue OpenCriticalAlerts { get; set; } = new();
    public KpiValue DataQualityPassRate { get; set; } = new();
    public KpiValue DegradedInfraComponents { get; set; } = new();
}

public sealed class ChartDataPoint
{
    public string Date { get; set; } = string.Empty;
    public double? Value { get; set; }
    public double? Value2 { get; set; }
}

public sealed class ChartSeries
{
    public string Name { get; set; } = string.Empty;
    public List<ChartDataPoint> DataPoints { get; set; } = new();
}

public sealed class DashboardChartsResponse
{
    public ChartSeries PipelineOutcomes { get; set; } = new() { Name = "pipelineOutcomes" };
    public ChartSeries IncidentVolume { get; set; } = new() { Name = "incidentVolume" };
    public ChartSeries DataQualityTrend { get; set; } = new() { Name = "dataQualityTrend" };
    public ChartSeries InfraHealthTrend { get; set; } = new() { Name = "infraHealthTrend" };
}
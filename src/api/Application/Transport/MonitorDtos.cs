using System.Text.Json.Serialization;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Transport;

/// <summary>WO-46: POST /api/v1/admin/monitors</summary>
public sealed class CreateMonitorRequest
{
    public string MonitorName { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MonitorEntityType EntityType { get; set; }

    public string EntityId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? BusinessPlanId { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public string? QueryTemplateId { get; set; }
    public int PollingFrequencyMinutes { get; set; }
    public List<AlertThresholdDto>? AlertThresholds { get; set; }
}

/// <summary>WO-46: PATCH /api/v1/admin/monitors/{id}</summary>
public sealed class UpdateMonitorRequest
{
    public string? MonitorName { get; set; }
    public string? QueryTemplateId { get; set; }
    public int? PollingFrequencyMinutes { get; set; }
    public List<AlertThresholdDto>? AlertThresholds { get; set; }
}

public sealed class AlertThresholdDto
{
    public string MetricName { get; set; } = string.Empty;
    public double WarningValue { get; set; }
    public double CriticalValue { get; set; }
    public string Operator { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
}

/// <summary>WO-46: single monitor response.</summary>
public sealed class MonitorResponse
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string MonitorName { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MonitorEntityType EntityType { get; set; }

    public string EntityId { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? BusinessPlanId { get; set; }
    public string? BusinessPlanName { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public string ConnectionName { get; set; } = string.Empty;
    public string? QueryTemplateId { get; set; }
    public string? QueryTemplateSnapshot { get; set; }
    public int PollingFrequencyMinutes { get; set; }
    public List<AlertThresholdDto>? AlertThresholds { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MonitorState Status { get; set; }

    public int SchemaVersion { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>WO-46: list response.</summary>
public sealed class MonitorListResponse
{
    public IReadOnlyList<MonitorResponse> Items { get; set; } = Array.Empty<MonitorResponse>();
    public int TotalCount { get; set; }
}

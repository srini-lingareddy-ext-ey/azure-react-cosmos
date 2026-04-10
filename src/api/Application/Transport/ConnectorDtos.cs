using System.Text.Json.Serialization;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Transport;

/// <summary>WO-47: POST /api/v1/connectors</summary>
public sealed class CreateConnectorRequest
{
    public string ConnectorName { get; set; } = string.Empty;
    public string ConnectorTypeId { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public IntegrationMode IntegrationMode { get; set; }

    public string? PollingScheduleCron { get; set; }
    public string Credentials { get; set; } = string.Empty;
    public List<FieldMappingDto>? FieldMappings { get; set; }
}

/// <summary>WO-47: PATCH /api/v1/connectors/{id}</summary>
public sealed class UpdateConnectorRequest
{
    public string? ConnectorName { get; set; }
    public string? PollingScheduleCron { get; set; }
    public string? Credentials { get; set; }
    public bool? IsEnabled { get; set; }
    public List<FieldMappingDto>? FieldMappings { get; set; }
}

public sealed class FieldMappingDto
{
    public string SourceField { get; set; } = string.Empty;
    public string TargetField { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TransformType TransformType { get; set; }

    public Dictionary<string, string>? ValueMap { get; set; }
}

/// <summary>WO-47: single connector response (excludes encrypted credentials).</summary>
public sealed class ConnectorResponse
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ConnectorName { get; set; } = string.Empty;
    public string ConnectorTypeId { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public IntegrationMode IntegrationMode { get; set; }

    public string? PollingScheduleCron { get; set; }
    public List<FieldMappingDto>? FieldMappings { get; set; }
    public int SchemaVersion { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public sealed class ConnectorListResponse
{
    public IReadOnlyList<ConnectorResponse> Items { get; set; } = Array.Empty<ConnectorResponse>();
    public int TotalCount { get; set; }
}

public sealed class ConnectorTestResponse
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

public sealed class ConnectorLogResponse
{
    public IReadOnlyList<ConnectorLogEntryDto> Entries { get; set; } = Array.Empty<ConnectorLogEntryDto>();
    public double SuccessRateLast30Cycles { get; set; }
}

public sealed class ConnectorLogEntryDto
{
    public string Id { get; set; } = string.Empty;
    public DateTimeOffset ExecutedAt { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ExecutionStatus Status { get; set; }

    public int EventsProduced { get; set; }
    public long DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>WO-47: catalog entry DTO.</summary>
public sealed class ConnectorTypeCatalogEntryDto
{
    public string ConnectorTypeId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public IntegrationMode IntegrationMode { get; set; }

    public string CertificationStatus { get; set; } = string.Empty;
    public string[] RequiredCredentialFields { get; set; } = [];
}

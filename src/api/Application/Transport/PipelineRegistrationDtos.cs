using System.Text.Json.Serialization;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Transport;

/// <summary>WO-43: POST /api/v1/admin/pipelines</summary>
public sealed class CreatePipelineRegistrationRequest
{
    public string PipelineName { get; set; } = string.Empty;
    public string? SourceSystem { get; set; }
    public string? TargetSystem { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MedallionLayer MedallionLayer { get; set; }

    public string? BusinessPlanId { get; set; }
    public string? Domain { get; set; }
    public string? Description { get; set; }
}

/// <summary>WO-43: PATCH /api/v1/admin/pipelines/{id}</summary>
public sealed class UpdatePipelineRegistrationRequest
{
    public string? PipelineName { get; set; }
    public string? SourceSystem { get; set; }
    public string? TargetSystem { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MedallionLayer? MedallionLayer { get; set; }

    public string? BusinessPlanId { get; set; }
    public string? Domain { get; set; }
    public string? Description { get; set; }
}

/// <summary>WO-43: single pipeline response.</summary>
public sealed class PipelineRegistrationResponse
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string PipelineName { get; set; } = string.Empty;
    public string? SourceSystem { get; set; }
    public string? TargetSystem { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MedallionLayer MedallionLayer { get; set; }

    public string? BusinessPlanId { get; set; }
    public string? BusinessPlanName { get; set; }
    public string? Domain { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int SchemaVersion { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>WO-43: list response.</summary>
public sealed class PipelineRegistrationListResponse
{
    public IReadOnlyList<PipelineRegistrationResponse> Items { get; set; } = Array.Empty<PipelineRegistrationResponse>();
    public int TotalCount { get; set; }
}

/// <summary>WO-43: deactivate response with suspended monitor count.</summary>
public sealed class PipelineDeactivateResponse
{
    public PipelineRegistrationResponse Pipeline { get; set; } = null!;
    public int MonitorsSuspended { get; set; }
}

using System.Text.Json.Serialization;

namespace Todo.Api.Application.Transport;

/// <summary>WO-42: POST /api/v1/admin/business-plans</summary>
public sealed class CreateBusinessPlanRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Domain { get; set; }
    public SLAWindowConfigDto? DefaultSlaWindow { get; set; }
}

/// <summary>WO-42: PATCH /api/v1/admin/business-plans/{id}</summary>
public sealed class UpdateBusinessPlanRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Domain { get; set; }
    public SLAWindowConfigDto? DefaultSlaWindow { get; set; }
}

public sealed class SLAWindowConfigDto
{
    public string WindowType { get; set; } = string.Empty;
    public int WindowValue { get; set; }
    public int AtRiskBufferMinutes { get; set; }
}

/// <summary>WO-42: single business plan response.</summary>
public sealed class BusinessPlanResponse
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Domain { get; set; }
    public bool IsActive { get; set; }
    public SLAWindowConfigDto? DefaultSlaWindow { get; set; }
    public int SchemaVersion { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>WO-42: paginated list response.</summary>
public sealed class BusinessPlanListResponse
{
    public IReadOnlyList<BusinessPlanResponse> Items { get; set; } = Array.Empty<BusinessPlanResponse>();
    public int TotalCount { get; set; }
}

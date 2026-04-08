using System.Text.Json.Serialization;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Transport;

/// <summary>WO-9: POST /api/v1/tenants</summary>
public sealed class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string? LogoUrl { get; set; }

    public string? BackgroundImageUrl { get; set; }
}

/// <summary>WO-9: PATCH /api/v1/tenants/{id}/config — partial update (merge).</summary>
public sealed class UpdateTenantConfigRequest
{
    public Dictionary<string, double>? HealthScoreWeights { get; set; }

    public HealthStatusThresholdsPatch? HealthStatusThresholds { get; set; }
}

/// <summary>Partial patch for <see cref="HealthStatusThresholds"/>.</summary>
public sealed class HealthStatusThresholdsPatch
{
    public double? HealthyMin { get; set; }

    public double? WarningMin { get; set; }

    public double? CriticalBelow { get; set; }
}

/// <summary>WO-9: single tenant response (writes return this document).</summary>
public sealed class TenantResponse
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TenantStatus Status { get; set; }

    public TenantBrandingDto? Branding { get; set; }

    public TenantConfigDto? Config { get; set; }

    public int SchemaVersion { get; set; }

    public DateTimeOffset? CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class TenantBrandingDto
{
    public string? LogoUrl { get; set; }

    public string? BackgroundImageUrl { get; set; }
}

public sealed class TenantConfigDto
{
    public Dictionary<string, double> HealthScoreWeights { get; set; } = new();

    public HealthStatusThresholdsDto? HealthStatusThresholds { get; set; }
}

public sealed class HealthStatusThresholdsDto
{
    public double? HealthyMin { get; set; }

    public double? WarningMin { get; set; }

    public double? CriticalBelow { get; set; }
}

/// <summary>Paginated list of tenants.</summary>
public sealed class TenantListResponse
{
    public IReadOnlyList<TenantResponse> Items { get; set; } = Array.Empty<TenantResponse>();

    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }
}

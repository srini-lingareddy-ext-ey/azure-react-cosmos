namespace Todo.Api.Application.Transport;

/// <summary>WO-45: POST /api/v1/admin/connections</summary>
public sealed class CreateConnectionRequest
{
    public string ConnectionName { get; set; } = string.Empty;
    public string ConnectorTypeId { get; set; } = string.Empty;
    public string Credentials { get; set; } = string.Empty;
}

/// <summary>WO-45: PATCH /api/v1/admin/connections/{id}</summary>
public sealed class UpdateConnectionRequest
{
    public string? ConnectionName { get; set; }
    public string? ConnectorTypeId { get; set; }
    public string? Credentials { get; set; }
    public bool? IsEnabled { get; set; }
}

/// <summary>WO-45: single connection response (excludes encrypted credentials).</summary>
public sealed class ConnectionResponse
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ConnectionName { get; set; } = string.Empty;
    public string ConnectorTypeId { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTimeOffset? LastTestedAt { get; set; }
    public string? LastTestResult { get; set; }
    public int SchemaVersion { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>WO-45: paginated list response.</summary>
public sealed class ConnectionListResponse
{
    public IReadOnlyList<ConnectionResponse> Items { get; set; } = Array.Empty<ConnectionResponse>();
    public int TotalCount { get; set; }
}

/// <summary>WO-45: POST /api/v1/admin/connections/{id}/test response.</summary>
public sealed class ConnectionTestResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}

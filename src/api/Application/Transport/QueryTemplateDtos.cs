namespace Todo.Api.Application.Transport;

/// <summary>WO-46: POST /api/v1/admin/query-templates</summary>
public sealed class CreateQueryTemplateRequest
{
    public string TemplateName { get; set; } = string.Empty;
    public string ConnectorTypeId { get; set; } = string.Empty;
    public string TemplateBody { get; set; } = string.Empty;
    public string[]? Parameters { get; set; }
}

/// <summary>WO-46: PATCH /api/v1/admin/query-templates/{id}</summary>
public sealed class UpdateQueryTemplateRequest
{
    public string? TemplateName { get; set; }
    public string? TemplateBody { get; set; }
    public string[]? Parameters { get; set; }

    /// <summary>"allExisting" propagates to all monitors using this template; "newOnly" (default) does not.</summary>
    public string? PropagationMode { get; set; }
}

/// <summary>WO-46: single query template response.</summary>
public sealed class QueryTemplateResponse
{
    public string Id { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string ConnectorTypeId { get; set; } = string.Empty;
    public string TemplateBody { get; set; } = string.Empty;
    public string[] Parameters { get; set; } = [];
    public bool IsActive { get; set; }
    public int SchemaVersion { get; set; }
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

/// <summary>WO-46: list response.</summary>
public sealed class QueryTemplateListResponse
{
    public IReadOnlyList<QueryTemplateResponse> Items { get; set; } = Array.Empty<QueryTemplateResponse>();
    public int TotalCount { get; set; }
}

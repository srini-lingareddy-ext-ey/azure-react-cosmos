using System.Text.Json;

namespace Todo.Api.Infrastructure.EventProcessing;

/// <summary>WO-81: handles lineage.analysis.request events from lineage-analysis-requests topic.</summary>
public sealed class LineageAnalysisEventHandler : IEventHandler
{
    private readonly Todo.Api.Application.Services.ILineageTraversalService _traversalService;
    private readonly ILogger<LineageAnalysisEventHandler> _logger;

    public string EventType => "lineage.analysis.request";

    public LineageAnalysisEventHandler(
        Todo.Api.Application.Services.ILineageTraversalService traversalService,
        ILogger<LineageAnalysisEventHandler> logger)
    {
        _traversalService = traversalService;
        _logger = logger;
    }

    public async Task HandleAsync(string payload, CancellationToken cancellationToken)
    {
        var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        var tenantId = root.GetProperty("tenantId").GetString() ?? string.Empty;
        var failedNodeId = root.GetProperty("failedNodeId").GetString() ?? string.Empty;
        string? incidentId = root.TryGetProperty("incidentId", out var incProp) ? incProp.GetString() : null;

        _logger.LogInformation("Processing lineage analysis request for node {NodeId} tenant {TenantId}", failedNodeId, tenantId);
        await _traversalService.ProcessAnalysisRequestAsync(tenantId, failedNodeId, incidentId, cancellationToken).ConfigureAwait(false);
    }
}
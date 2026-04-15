namespace Todo.Api.Application.Services;

/// <summary>WO-81: Bidirectional BFS lineage traversal with status enrichment.</summary>
public interface ILineageTraversalService
{
    Task ProcessAnalysisRequestAsync(string tenantId, string failedNodeId, string? incidentId, CancellationToken ct = default);
}
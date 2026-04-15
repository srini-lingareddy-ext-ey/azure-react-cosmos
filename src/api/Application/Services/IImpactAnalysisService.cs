using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Services;

/// <summary>WO-85: Lineage Impact Analysis API.</summary>
public interface IImpactAnalysisService
{
    Task<ImpactAnalysisResult?> GetByIncidentIdAsync(string incidentId, string tenantId, CancellationToken ct = default);
    Task<ImpactAnalysisResult?> GetByPipelineIdAsync(string pipelineId, string tenantId, CancellationToken ct = default);
    Task<ImpactAnalysisResult> TriggerAnalysisAsync(string tenantId, string failedNodeId, string? incidentId, CancellationToken ct = default);
    Task TriggerLineageRefreshAsync(string tenantId, CancellationToken ct = default);
}
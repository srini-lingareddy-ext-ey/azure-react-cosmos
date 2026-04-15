using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IImpactAnalysisRepository
{
    Task<ImpactAnalysisResult?> GetByAnalysisIdAsync(string analysisId, string tenantId, CancellationToken ct = default);
    Task<ImpactAnalysisResult?> GetByIncidentIdAsync(string incidentId, string tenantId, CancellationToken ct = default);
    Task<ImpactAnalysisResult?> GetLatestByPipelineIdAsync(string pipelineId, string tenantId, CancellationToken ct = default);
    Task<ImpactAnalysisResult> CreateAsync(ImpactAnalysisResult entity, CancellationToken ct = default);
    Task<ImpactAnalysisResult> UpdateAsync(ImpactAnalysisResult entity, CancellationToken ct = default);
}

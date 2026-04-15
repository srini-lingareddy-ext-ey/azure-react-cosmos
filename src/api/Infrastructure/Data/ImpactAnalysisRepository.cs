using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed impact analysis repository (WO-77). Partition key /tenantId.</summary>
public sealed class ImpactAnalysisRepository : IImpactAnalysisRepository
{
    private readonly IRepository<ImpactAnalysisResult> _repository;
    public ImpactAnalysisRepository(IRepository<ImpactAnalysisResult> repository) { _repository = repository; }

    public Task<ImpactAnalysisResult?> GetByAnalysisIdAsync(string analysisId, string tenantId, CancellationToken ct = default) =>
        _repository.GetByIdAsync(analysisId, tenantId, ct);

    public async Task<ImpactAnalysisResult?> GetByIncidentIdAsync(string incidentId, string tenantId, CancellationToken ct = default)
    {
        var spec = new QuerySpec(
            "SELECT TOP 1 * FROM c WHERE c.tenantId = @tenantId AND c.incidentId = @incidentId ORDER BY c.createdAt DESC",
            new Dictionary<string, object> { ["@tenantId"] = tenantId, ["@incidentId"] = incidentId });
        await foreach (var row in _repository.QueryAsync(spec, ct).ConfigureAwait(false))
            return row;
        return null;
    }

    public async Task<ImpactAnalysisResult?> GetLatestByPipelineIdAsync(string pipelineId, string tenantId, CancellationToken ct = default)
    {
        var spec = new QuerySpec(
            "SELECT TOP 1 * FROM c WHERE c.tenantId = @tenantId AND c.failedNodeId = @pipelineId ORDER BY c.createdAt DESC",
            new Dictionary<string, object> { ["@tenantId"] = tenantId, ["@pipelineId"] = pipelineId });
        await foreach (var row in _repository.QueryAsync(spec, ct).ConfigureAwait(false))
            return row;
        return null;
    }

    public Task<ImpactAnalysisResult> CreateAsync(ImpactAnalysisResult entity, CancellationToken ct = default) =>
        _repository.CreateAsync(entity, ct);

    public Task<ImpactAnalysisResult> UpdateAsync(ImpactAnalysisResult entity, CancellationToken ct = default) =>
        _repository.UpdateAsync(entity, ct);
}

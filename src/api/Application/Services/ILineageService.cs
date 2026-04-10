using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

/// <summary>WO-44: pipeline lineage relationships with cycle detection.</summary>
public interface ILineageService
{
    Task<PipelineLineageResponse> GetLineageAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default);
    Task<LineageRelationshipResponse> CreateAsync(string userId, string tenantId, CreateLineageRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(string relationshipId, string tenantId, CancellationToken cancellationToken = default);
}

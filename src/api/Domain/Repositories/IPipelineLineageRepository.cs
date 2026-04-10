using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

/// <summary>Persistence for <see cref="PipelineLineageRelationship"/> (WO-16).</summary>
public interface IPipelineLineageRepository
{
    Task<PipelineLineageRelationship?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<PipelineLineageRelationship> GetByUpstreamPipelineAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<PipelineLineageRelationship> GetByDownstreamPipelineAsync(string pipelineId, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<PipelineLineageRelationship> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<PipelineLineageRelationship> CreateAsync(PipelineLineageRelationship relationship, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, string tenantId, string? etag = null, CancellationToken cancellationToken = default);
}

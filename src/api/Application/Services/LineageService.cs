using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>WO-44: lineage service with depth-first cycle detection.</summary>
public sealed class LineageService : ILineageService
{
    private readonly IPipelineLineageRepository _lineageRepo;
    private readonly IPipelineRegistrationRepository _pipelineRepo;

    public LineageService(
        IPipelineLineageRepository lineageRepo,
        IPipelineRegistrationRepository pipelineRepo)
    {
        _lineageRepo = lineageRepo;
        _pipelineRepo = pipelineRepo;
    }

    public async Task<PipelineLineageResponse> GetLineageAsync(
        string pipelineId, string tenantId, CancellationToken cancellationToken = default)
    {
        var upstream = new List<LineageEdgeDto>();
        await foreach (var r in _lineageRepo.GetByDownstreamPipelineAsync(pipelineId, tenantId, cancellationToken).ConfigureAwait(false))
        {
            upstream.Add(new LineageEdgeDto
            {
                RelationshipId = r.Id,
                RelatedPipelineId = r.UpstreamPipelineId,
                RelatedPipelineName = r.UpstreamPipelineName,
            });
        }

        var downstream = new List<LineageEdgeDto>();
        await foreach (var r in _lineageRepo.GetByUpstreamPipelineAsync(pipelineId, tenantId, cancellationToken).ConfigureAwait(false))
        {
            downstream.Add(new LineageEdgeDto
            {
                RelationshipId = r.Id,
                RelatedPipelineId = r.DownstreamPipelineId,
                RelatedPipelineName = r.DownstreamPipelineName,
            });
        }

        return new PipelineLineageResponse
        {
            PipelineId = pipelineId,
            Upstream = upstream,
            Downstream = downstream,
        };
    }

    public async Task<LineageRelationshipResponse> CreateAsync(
        string userId, string tenantId, CreateLineageRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.UpstreamPipelineId == request.DownstreamPipelineId)
            throw new InvalidOperationException("A pipeline cannot be its own upstream.");

        var upstream = await _pipelineRepo.GetByIdAsync(request.UpstreamPipelineId, tenantId, cancellationToken).ConfigureAwait(false);
        if (upstream is null)
            throw new InvalidOperationException("Upstream pipeline not found in this tenant.");

        var downstream = await _pipelineRepo.GetByIdAsync(request.DownstreamPipelineId, tenantId, cancellationToken).ConfigureAwait(false);
        if (downstream is null)
            throw new InvalidOperationException("Downstream pipeline not found in this tenant.");

        // Check for duplicate
        await foreach (var existing in _lineageRepo.GetByUpstreamPipelineAsync(request.UpstreamPipelineId, tenantId, cancellationToken).ConfigureAwait(false))
        {
            if (existing.DownstreamPipelineId == request.DownstreamPipelineId)
                throw new InvalidOperationException("This lineage relationship already exists.");
        }

        // Depth-first cycle detection: if adding upstream->downstream edge, check whether downstream can reach upstream
        var cyclePath = await DetectCycleAsync(request.DownstreamPipelineId, request.UpstreamPipelineId, tenantId, cancellationToken).ConfigureAwait(false);
        if (cyclePath is not null)
        {
            throw new InvalidOperationException(
                "Adding this relationship would create a cycle: " + string.Join(" -> ", cyclePath));
        }

        var entity = new PipelineLineageRelationship
        {
            Id = Guid.NewGuid().ToString("n"),
            TenantId = tenantId,
            UpstreamPipelineId = request.UpstreamPipelineId,
            UpstreamPipelineName = upstream.PipelineName,
            DownstreamPipelineId = request.DownstreamPipelineId,
            DownstreamPipelineName = downstream.PipelineName,
            SchemaVersion = 1,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
            UpdatedBy = userId,
        };

        var created = await _lineageRepo.CreateAsync(entity, cancellationToken).ConfigureAwait(false);

        return new LineageRelationshipResponse
        {
            Id = created.Id,
            TenantId = created.TenantId,
            UpstreamPipelineId = created.UpstreamPipelineId,
            UpstreamPipelineName = created.UpstreamPipelineName,
            DownstreamPipelineId = created.DownstreamPipelineId,
            DownstreamPipelineName = created.DownstreamPipelineName,
            CreatedAt = created.CreatedAt,
            CreatedBy = created.CreatedBy,
        };
    }

    public async Task DeleteAsync(
        string relationshipId, string tenantId, CancellationToken cancellationToken = default)
    {
        var rel = await _lineageRepo.GetByIdAsync(relationshipId, tenantId, cancellationToken).ConfigureAwait(false);
        if (rel is null) throw new KeyNotFoundException("Lineage relationship not found.");
        await _lineageRepo.DeleteAsync(relationshipId, tenantId, rel.Etag, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// DFS from startId following downstream edges. Returns cycle path if targetId is reachable, null otherwise.
    /// </summary>
    private async Task<List<string>?> DetectCycleAsync(
        string startId, string targetId, string tenantId, CancellationToken cancellationToken)
    {
        var visited = new HashSet<string>();
        var path = new List<string>();

        async Task<bool> DfsAsync(string current)
        {
            if (current == targetId)
            {
                path.Add(current);
                return true;
            }

            if (!visited.Add(current)) return false;
            path.Add(current);

            await foreach (var edge in _lineageRepo.GetByUpstreamPipelineAsync(current, tenantId, cancellationToken).ConfigureAwait(false))
            {
                if (await DfsAsync(edge.DownstreamPipelineId).ConfigureAwait(false))
                    return true;
            }

            path.RemoveAt(path.Count - 1);
            return false;
        }

        return await DfsAsync(startId).ConfigureAwait(false) ? path : null;
    }
}

using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>WO-81: BFS traversal of LineageNode graph with status enrichment.</summary>
public sealed class LineageTraversalService : ILineageTraversalService
{
    private const int MaxDepth = 10;
    private readonly ILineageNodeRepository _nodeRepo;
    private readonly IImpactAnalysisRepository _resultRepo;
    private readonly IPipelineStatusSummaryRepository _pipelineStatusRepo;
    private readonly IComponentHealthStatusRepository _infraStatusRepo;
    private readonly IIncidentRepository _incidentRepo;
    private readonly ILogger<LineageTraversalService> _logger;

    public LineageTraversalService(
        ILineageNodeRepository nodeRepo,
        IImpactAnalysisRepository resultRepo,
        IPipelineStatusSummaryRepository pipelineStatusRepo,
        IComponentHealthStatusRepository infraStatusRepo,
        IIncidentRepository incidentRepo,
        ILogger<LineageTraversalService> logger)
    {
        _nodeRepo = nodeRepo;
        _resultRepo = resultRepo;
        _pipelineStatusRepo = pipelineStatusRepo;
        _infraStatusRepo = infraStatusRepo;
        _incidentRepo = incidentRepo;
        _logger = logger;
    }

    public async Task ProcessAnalysisRequestAsync(string tenantId, string failedNodeId, string? incidentId, CancellationToken ct = default)
    {
        var result = new ImpactAnalysisResult
        {
            TenantId = tenantId,
            FailedNodeId = failedNodeId,
            IncidentId = incidentId,
            Status = ImpactAnalysisStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _resultRepo.CreateAsync(result, ct).ConfigureAwait(false);

        var rootNode = await _nodeRepo.GetByNodeIdAsync(failedNodeId, tenantId, ct).ConfigureAwait(false);
        if (rootNode is null)
        {
            result.Status = ImpactAnalysisStatus.Unavailable;
            result.TraversedAt = DateTimeOffset.UtcNow;
            await _resultRepo.UpdateAsync(result, ct).ConfigureAwait(false);
            return;
        }

        result.FailedNodeType = rootNode.NodeType;

        var upstreamNodes = await TraverseAsync(tenantId, failedNodeId, upstream: true, ct).ConfigureAwait(false);
        var downstreamNodes = await TraverseAsync(tenantId, failedNodeId, upstream: false, ct).ConfigureAwait(false);

        result.Upstream = upstreamNodes.Nodes;
        result.AdditionalUpstreamExist = upstreamNodes.MaxDepthReached;
        result.Downstream = downstreamNodes.Nodes;
        result.AdditionalDownstreamExist = downstreamNodes.MaxDepthReached;
        result.AffectedDownstreamCount = downstreamNodes.Nodes.Count;
        result.Status = ImpactAnalysisStatus.Complete;
        result.TraversedAt = DateTimeOffset.UtcNow;

        await _resultRepo.UpdateAsync(result, ct).ConfigureAwait(false);

        if (incidentId is not null)
        {
            try
            {
                var incident = await _incidentRepo.GetByIdAsync(incidentId, tenantId, ct).ConfigureAwait(false);
                if (incident is not null)
                {
                    incident.LineageAnalysisAvailable = true;
                    await _incidentRepo.UpdateAsync(incident, ct).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update incident {IncidentId} lineage flag", incidentId);
            }
        }

        _logger.LogInformation("Lineage analysis complete for node {NodeId} tenant {TenantId}: {Up} upstream, {Down} downstream", failedNodeId, tenantId, upstreamNodes.Nodes.Count, downstreamNodes.Nodes.Count);
    }

    private async Task<TraversalResult> TraverseAsync(string tenantId, string startNodeId, bool upstream, CancellationToken ct)
    {
        var visited = new HashSet<string> { startNodeId };
        var queue = new Queue<(string NodeId, int Depth)>();
        var results = new List<ImpactNode>();
        bool maxDepthReached = false;

        var startNode = await _nodeRepo.GetByNodeIdAsync(startNodeId, tenantId, ct).ConfigureAwait(false);
        if (startNode is null) return new TraversalResult(results, false);

        var neighbors = upstream ? startNode.UpstreamIds : startNode.DownstreamIds;
        foreach (var n in neighbors)
        {
            if (visited.Add(n))
                queue.Enqueue((n, 1));
        }

        while (queue.Count > 0)
        {
            var (nodeId, depth) = queue.Dequeue();
            if (depth > MaxDepth)
            {
                maxDepthReached = true;
                continue;
            }

            var node = await _nodeRepo.GetByNodeIdAsync(nodeId, tenantId, ct).ConfigureAwait(false);
            if (node is null) continue;

            var status = await GetNodeStatusAsync(nodeId, node.NodeType, tenantId, ct).ConfigureAwait(false);
            results.Add(new ImpactNode
            {
                NodeId = node.NodeId,
                NodeName = node.NodeName,
                NodeType = node.NodeType,
                CurrentStatus = status,
                Depth = depth
            });

            var nextNeighbors = upstream ? node.UpstreamIds : node.DownstreamIds;
            foreach (var n in nextNeighbors)
            {
                if (visited.Add(n))
                    queue.Enqueue((n, depth + 1));
            }
        }

        return new TraversalResult(results, maxDepthReached);
    }

    private async Task<ImpactNodeStatus> GetNodeStatusAsync(string nodeId, LineageNodeType nodeType, string tenantId, CancellationToken ct)
    {
        if (nodeType == LineageNodeType.Pipeline)
        {
            var ps = await _pipelineStatusRepo.GetByIdAsync(nodeId, tenantId, ct).ConfigureAwait(false);
            if (ps is null) return ImpactNodeStatus.Unknown;
            return ps.Status.ToLowerInvariant() switch
            {
                "failed" => ImpactNodeStatus.Failed,
                "running" or "healthy" or "succeeded" => ImpactNodeStatus.Healthy,
                "warning" or "degraded" => ImpactNodeStatus.AtRisk,
                _ => ImpactNodeStatus.Unknown
            };
        }

        var comp = await _infraStatusRepo.GetByIdAsync(nodeId, tenantId, ct).ConfigureAwait(false);
        if (comp is null) return ImpactNodeStatus.Unknown;
        return comp.Status switch
        {
            InfraHealthState.Healthy => ImpactNodeStatus.Healthy,
            InfraHealthState.Warning => ImpactNodeStatus.AtRisk,
            InfraHealthState.Critical => ImpactNodeStatus.Failed,
            _ => ImpactNodeStatus.Unknown
        };
    }

    private sealed record TraversalResult(List<ImpactNode> Nodes, bool MaxDepthReached);
}
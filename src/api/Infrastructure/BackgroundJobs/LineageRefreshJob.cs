using System.Diagnostics;
using Todo.Api.Application.EventPublishing;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.BackgroundJobs;

/// <summary>WO-80: daily job - rebuilds LineageNode graph from PipelineLineageRelationship per tenant.</summary>
public sealed class LineageRefreshJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LineageRefreshJob> _logger;

    public LineageRefreshJob(IServiceProvider serviceProvider, ILogger<LineageRefreshJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(24));
        _logger.LogInformation("LineageRefreshJob started (daily cycle)");

        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await RunCycleAsync(scope.ServiceProvider, _logger, stoppingToken).ConfigureAwait(false);
                _logger.LogDebug("Lineage refresh cycle completed");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Lineage refresh cycle failed");
            }
        }
    }

    internal static async Task RunCycleAsync(IServiceProvider sp, ILogger logger, CancellationToken ct)
    {
        var tenantRepo = sp.GetRequiredService<ITenantRepository>();
        var lineageRelRepo = sp.GetRequiredService<IPipelineLineageRepository>();
        var lineageNodeRepo = sp.GetRequiredService<ILineageNodeRepository>();
        var refreshStatusRepo = sp.GetRequiredService<ILineageRefreshStatusRepository>();
        var pipelineRepo = sp.GetRequiredService<IPipelineRegistrationRepository>();
        var eventPublisher = sp.GetRequiredService<IEventPublisher>();

        await foreach (var tenant in tenantRepo.GetAllAsync(ct).ConfigureAwait(false))
        {
            var sw = Stopwatch.StartNew();
            try
            {
                var relationships = new List<PipelineLineageRelationship>();
                await foreach (var r in lineageRelRepo.GetAllByTenantAsync(tenant.Id, ct).ConfigureAwait(false))
                    relationships.Add(r);

                var nodeMap = new Dictionary<string, LineageNode>();

                var pipelines = new List<PipelineRegistration>();
                await foreach (var p in pipelineRepo.GetAllByTenantAsync(tenant.Id, ct).ConfigureAwait(false))
                    pipelines.Add(p);

                foreach (var p in pipelines)
                {
                    if (!nodeMap.ContainsKey(p.Id))
                    {
                        nodeMap[p.Id] = new LineageNode
                        {
                            Id = p.Id, TenantId = tenant.Id, NodeId = p.Id,
                            NodeName = p.PipelineName, NodeType = LineageNodeType.Pipeline,
                            LastRefreshedAt = DateTimeOffset.UtcNow
                        };
                    }
                }

                foreach (var rel in relationships)
                {
                    EnsureNode(nodeMap, tenant.Id, rel.UpstreamPipelineId, rel.UpstreamPipelineName);
                    EnsureNode(nodeMap, tenant.Id, rel.DownstreamPipelineId, rel.DownstreamPipelineName);

                    var upstream = nodeMap[rel.UpstreamPipelineId];
                    var downstream = nodeMap[rel.DownstreamPipelineId];

                    if (!upstream.DownstreamIds.Contains(rel.DownstreamPipelineId))
                        upstream.DownstreamIds.Add(rel.DownstreamPipelineId);
                    if (!downstream.UpstreamIds.Contains(rel.UpstreamPipelineId))
                        downstream.UpstreamIds.Add(rel.UpstreamPipelineId);
                }

                var nodes = nodeMap.Values.ToList();
                await lineageNodeRepo.DeleteAllByTenantAsync(tenant.Id, ct).ConfigureAwait(false);
                if (nodes.Count > 0)
                    await lineageNodeRepo.BulkUpsertAsync(nodes, tenant.Id, ct).ConfigureAwait(false);

                sw.Stop();
                await refreshStatusRepo.UpsertAsync(new LineageRefreshStatus
                {
                    Id = tenant.Id, TenantId = tenant.Id,
                    LastRefreshedAt = DateTimeOffset.UtcNow,
                    LastRefreshStatus = RefreshStatus.Success,
                    NodeCount = nodes.Count,
                    RefreshDurationSeconds = sw.Elapsed.TotalSeconds
                }, ct).ConfigureAwait(false);

                logger.LogInformation("Lineage refresh for tenant {TenantId}: {NodeCount} nodes in {Duration:F1}s", tenant.Id, nodes.Count, sw.Elapsed.TotalSeconds);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                sw.Stop();
                logger.LogError(ex, "Lineage refresh failed for tenant {TenantId}", tenant.Id);

                await refreshStatusRepo.UpsertAsync(new LineageRefreshStatus
                {
                    Id = tenant.Id, TenantId = tenant.Id,
                    LastRefreshStatus = RefreshStatus.Failed,
                    LastErrorMessage = ex.Message,
                    RefreshDurationSeconds = sw.Elapsed.TotalSeconds
                }, ct).ConfigureAwait(false);

                try
                {
                    await eventPublisher.PublishAsync("platform-alerts", new NormalizedEvent
                    {
                        EventType = "lineage.refresh.failed",
                        TenantId = tenant.Id,
                        Payload = System.Text.Json.JsonSerializer.Serialize(new { error = ex.Message }),
                        Timestamp = DateTimeOffset.UtcNow
                    }, ct).ConfigureAwait(false);
                }
                catch (Exception pubEx)
                {
                    logger.LogError(pubEx, "Failed to publish lineage refresh failure alert for tenant {TenantId}", tenant.Id);
                }
            }
        }
    }

    private static void EnsureNode(Dictionary<string, LineageNode> map, string tenantId, string nodeId, string nodeName)
    {
        if (!map.ContainsKey(nodeId))
        {
            map[nodeId] = new LineageNode
            {
                Id = nodeId, TenantId = tenantId, NodeId = nodeId,
                NodeName = nodeName, NodeType = LineageNodeType.Pipeline,
                LastRefreshedAt = DateTimeOffset.UtcNow
            };
        }
    }
}
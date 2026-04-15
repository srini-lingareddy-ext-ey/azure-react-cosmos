using System.Text.Json;
using Todo.Api.Application.EventPublishing;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

/// <summary>WO-85: reads ImpactAnalysisResult, triggers analysis events, dispatches lineage refresh.</summary>
public sealed class ImpactAnalysisService : IImpactAnalysisService
{
    private readonly IImpactAnalysisRepository _resultRepo;
    private readonly IIncidentRepository _incidentRepo;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ImpactAnalysisService> _logger;

    public ImpactAnalysisService(
        IImpactAnalysisRepository resultRepo,
        IIncidentRepository incidentRepo,
        IEventPublisher eventPublisher,
        ILogger<ImpactAnalysisService> logger)
    {
        _resultRepo = resultRepo;
        _incidentRepo = incidentRepo;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<ImpactAnalysisResult?> GetByIncidentIdAsync(string incidentId, string tenantId, CancellationToken ct = default)
    {
        return await _resultRepo.GetByIncidentIdAsync(incidentId, tenantId, ct).ConfigureAwait(false);
    }

    public async Task<ImpactAnalysisResult?> GetByPipelineIdAsync(string pipelineId, string tenantId, CancellationToken ct = default)
    {
        return await _resultRepo.GetLatestByPipelineIdAsync(pipelineId, tenantId, ct).ConfigureAwait(false);
    }

    public async Task<ImpactAnalysisResult> TriggerAnalysisAsync(string tenantId, string failedNodeId, string? incidentId, CancellationToken ct = default)
    {
        if (incidentId is not null)
        {
            var incident = await _incidentRepo.GetByIdAsync(incidentId, tenantId, ct).ConfigureAwait(false)
                ?? throw new KeyNotFoundException($"Incident {incidentId} not found.");
        }

        var result = new ImpactAnalysisResult
        {
            TenantId = tenantId,
            FailedNodeId = failedNodeId,
            IncidentId = incidentId,
            Status = ImpactAnalysisStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _resultRepo.CreateAsync(result, ct).ConfigureAwait(false);

        var payload = JsonSerializer.Serialize(new { tenantId, failedNodeId, incidentId, analysisId = result.Id });
        await _eventPublisher.PublishAsync("lineage-analysis-requests", new NormalizedEvent
        {
            EventType = "lineage.analysis.request",
            TenantId = tenantId,
            Payload = payload,
            Timestamp = DateTimeOffset.UtcNow
        }, ct).ConfigureAwait(false);

        _logger.LogInformation("Triggered lineage analysis {AnalysisId} for node {NodeId}", result.Id, failedNodeId);
        return result;
    }

    public async Task TriggerLineageRefreshAsync(string tenantId, CancellationToken ct = default)
    {
        await _eventPublisher.PublishAsync("platform-alerts", new NormalizedEvent
        {
            EventType = "lineage.refresh.requested",
            TenantId = tenantId,
            Payload = JsonSerializer.Serialize(new { tenantId, triggeredAt = DateTimeOffset.UtcNow }),
            Timestamp = DateTimeOffset.UtcNow
        }, ct).ConfigureAwait(false);
        _logger.LogInformation("Lineage refresh triggered for tenant {TenantId}", tenantId);
    }
}
using Todo.Api.Application.EventPublishing;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;


namespace Todo.Api.Application.Services;

public sealed class DataQualityEvaluationService : IDataQualityEvaluationService
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IDataQualityStatusRepository _statusRepo;
    private readonly IDataQualityThresholdConfigRepository _configRepo;
    private readonly IPipelineStatusSummaryRepository _summaryRepo;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<DataQualityEvaluationService> _logger;

    public DataQualityEvaluationService(
        ITenantRepository tenantRepo,
        IDataQualityStatusRepository statusRepo,
        IDataQualityThresholdConfigRepository configRepo,
        IPipelineStatusSummaryRepository summaryRepo,
        IEventPublisher eventPublisher,
        ILogger<DataQualityEvaluationService> logger)
    {
        _tenantRepo = tenantRepo;
        _statusRepo = statusRepo;
        _configRepo = configRepo;
        _summaryRepo = summaryRepo;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task EvaluateAllAsync(CancellationToken ct)
    {
        await foreach (var tenant in _tenantRepo.GetAllAsync(ct).ConfigureAwait(false))
        {
            await EvaluateTenantAsync(tenant.Id, ct).ConfigureAwait(false);
        }
    }

    private async Task EvaluateTenantAsync(string tenantId, CancellationToken ct)
    {
        var statuses = new List<DataQualityStatus>();
        await foreach (var s in _statusRepo.GetAllByTenantAsync(tenantId, ct).ConfigureAwait(false))
            statuses.Add(s);

        foreach (var status in statuses)
        {
            var config = await _configRepo.GetByPipelineIdAsync(status.PipelineId, tenantId, ct).ConfigureAwait(false);
            if (config is null) continue;

            var previousQuality = status.QualityStatusValue;
            var previousLatency = status.LatencyStatusValue;

            // Quality evaluation
            if (status.QualityScore.HasValue)
            {
                var score = status.QualityScore.Value;
                var warning = config.WarningThreshold ?? 85;
                var critical = config.CriticalThreshold ?? 70;
                status.QualityStatusValue = score >= warning ? QualityStatus.Passing
                    : score >= critical ? QualityStatus.Warning
                    : QualityStatus.Failing;
            }

            // Latency evaluation
            var summary = await _summaryRepo.GetByIdAsync(status.PipelineId, tenantId, ct).ConfigureAwait(false);
            if (summary?.LastRunAt.HasValue == true && config.FreshnessThresholdSeconds.HasValue)
            {
                var elapsed = (DateTimeOffset.UtcNow - summary.LastRunAt.Value).TotalSeconds;
                var threshold = (double)config.FreshnessThresholdSeconds.Value;
                var bufferPercent = config.FreshnessBufferPercent ?? 20;
                var approachingStart = threshold * (1 - bufferPercent / 100.0);

                status.LatencyStatusValue = elapsed > threshold ? LatencyStatus.Stale
                    : elapsed >= approachingStart ? LatencyStatus.Approaching
                    : LatencyStatus.Fresh;
                status.LastSuccessfulRunAt = summary.LastRunAt;
            }

            status.EvaluatedAt = DateTimeOffset.UtcNow;
            await _statusRepo.UpsertAsync(status, ct).ConfigureAwait(false);

            // Publish events on transitions
            if (status.QualityStatusValue != previousQuality)
            {
                var classification = status.QualityStatusValue == QualityStatus.Failing ? "incident" : "warning";
                if (status.QualityStatusValue is QualityStatus.Warning or QualityStatus.Failing)
                {
                    await PublishEventAsync($"dataQuality.{classification}", tenantId, status.PipelineId, ct).ConfigureAwait(false);
                }
            }

            if (status.LatencyStatusValue != previousLatency && status.LatencyStatusValue is LatencyStatus.Stale or LatencyStatus.Approaching)
            {
                await PublishEventAsync("dataQuality.latencyAlert", tenantId, status.PipelineId, ct).ConfigureAwait(false);
            }
        }
    }

    private async Task PublishEventAsync(string eventType, string tenantId, string pipelineId, CancellationToken ct)
    {
        var evt = new NormalizedEvent
        {
            EventType = eventType,
            TenantId = tenantId,
            Payload = System.Text.Json.JsonSerializer.Serialize(new { tenantId, pipelineId, timestamp = DateTimeOffset.UtcNow }),
        };
        await _eventPublisher.PublishAsync("quality-events", evt, ct).ConfigureAwait(false);
    }
}

using Todo.Api.Application.EventPublishing;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;


namespace Todo.Api.Application.Services;

public sealed class SLAEvaluationService : ISLAEvaluationService
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IPipelineSLAConfigRepository _configRepo;
    private readonly IPipelineSLAStatusRepository _statusRepo;
    private readonly IPipelineSLABreachRecordRepository _breachRepo;
    private readonly IPipelineStatusSummaryRepository _summaryRepo;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<SLAEvaluationService> _logger;

    public SLAEvaluationService(
        ITenantRepository tenantRepo,
        IPipelineSLAConfigRepository configRepo,
        IPipelineSLAStatusRepository statusRepo,
        IPipelineSLABreachRecordRepository breachRepo,
        IPipelineStatusSummaryRepository summaryRepo,
        IEventPublisher eventPublisher,
        ILogger<SLAEvaluationService> logger)
    {
        _tenantRepo = tenantRepo;
        _configRepo = configRepo;
        _statusRepo = statusRepo;
        _breachRepo = breachRepo;
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
        var configs = new List<PipelineSLAConfig>();
        await foreach (var c in _configRepo.GetAllWithConfigByTenantAsync(tenantId, ct).ConfigureAwait(false))
            configs.Add(c);

        foreach (var config in configs)
        {
            var summary = await _summaryRepo.GetByIdAsync(config.PipelineId, tenantId, ct).ConfigureAwait(false);
            var existingStatus = await _statusRepo.GetByPipelineIdAsync(config.PipelineId, tenantId, ct).ConfigureAwait(false);
            var previousState = existingStatus?.Status ?? SLAStatus.OnTrack;

            var now = DateTimeOffset.UtcNow;
            var slaDeadline = ResolveSLADeadline(config, now);
            var lastRunAt = summary?.LastRunAt;
            var pipelineCompleted = lastRunAt.HasValue && lastRunAt.Value.Date == now.Date;

            SLAStatus newStatus;
            double? timeRemaining = null;

            if (pipelineCompleted)
            {
                newStatus = SLAStatus.Met;
            }
            else if (now >= slaDeadline)
            {
                newStatus = SLAStatus.Breached;
            }
            else if (now >= slaDeadline.AddMinutes(-config.AtRiskBufferMinutes))
            {
                newStatus = SLAStatus.AtRisk;
                timeRemaining = (slaDeadline - now).TotalSeconds;
            }
            else
            {
                newStatus = SLAStatus.OnTrack;
                timeRemaining = (slaDeadline - now).TotalSeconds;
            }

            var status = existingStatus ?? new PipelineSLAStatus
            {
                Id = config.PipelineId,
                TenantId = tenantId,
                PipelineId = config.PipelineId,
            };
            status.PipelineName = summary?.PipelineName ?? string.Empty;
            status.Status = newStatus;
            status.TimeRemainingSeconds = timeRemaining;
            status.LastRunAt = lastRunAt;
            status.EvaluatedAt = now;

            await _statusRepo.UpsertAsync(status, ct).ConfigureAwait(false);

            if (newStatus == SLAStatus.Breached && previousState != SLAStatus.Breached)
            {
                var breach = new PipelineSLABreachRecord
                {
                    TenantId = tenantId,
                    PipelineId = config.PipelineId,
                    PipelineName = status.PipelineName,
                    SlaWindowClosedAt = slaDeadline,
                    BreachDetectedAt = now,
                };
                await _breachRepo.CreateAsync(breach, ct).ConfigureAwait(false);
                await PublishEventAsync("sla.breached", tenantId, config.PipelineId, ct).ConfigureAwait(false);
            }
            else if (newStatus == SLAStatus.AtRisk && previousState == SLAStatus.OnTrack)
            {
                await PublishEventAsync("sla.atRisk", tenantId, config.PipelineId, ct).ConfigureAwait(false);
            }

            if (newStatus == SLAStatus.Met && previousState == SLAStatus.Breached)
            {
                await foreach (var br in _breachRepo.GetByPipelineIdAsync(config.PipelineId, tenantId, 1, ct).ConfigureAwait(false))
                {
                    if (br.CompletedAt is null)
                    {
                        var overdue = lastRunAt.HasValue ? (int)(lastRunAt.Value - slaDeadline).TotalMinutes : 0;
                        await _breachRepo.UpdateCompletedAtAsync(br.Id, tenantId, now, overdue, ct).ConfigureAwait(false);
                    }
                    break;
                }
            }
        }
    }

    private static DateTimeOffset ResolveSLADeadline(PipelineSLAConfig config, DateTimeOffset now)
    {
        if (config.WindowType == SLAWindowType.AbsoluteTime && TimeOnly.TryParse(config.WindowValue, out var time))
            return new DateTimeOffset(now.Date.Add(time.ToTimeSpan()), TimeSpan.Zero);
        if (config.WindowType == SLAWindowType.Duration && double.TryParse(config.WindowValue, out var minutes))
            return now.AddMinutes(minutes);
        return now.AddHours(24);
    }

    private async Task PublishEventAsync(string eventType, string tenantId, string pipelineId, CancellationToken ct)
    {
        var evt = new NormalizedEvent
        {
            EventType = eventType,
            TenantId = tenantId,
            Payload = System.Text.Json.JsonSerializer.Serialize(new { tenantId, pipelineId, timestamp = DateTimeOffset.UtcNow }),
        };
        await _eventPublisher.PublishAsync("pipeline-events", evt, ct).ConfigureAwait(false);
    }
}

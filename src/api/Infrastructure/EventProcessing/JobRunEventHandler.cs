using System.Text.Json;
using Todo.Api.Application.EventPublishing;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;


namespace Todo.Api.Infrastructure.EventProcessing;

public sealed class JobRunEventHandler : IEventHandler
{
    public string EventType => "job.run";

    private readonly IJobRunRepository _jobRunRepo;
    private readonly IJobLongRunThresholdRepository _thresholdRepo;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<JobRunEventHandler> _logger;

    public JobRunEventHandler(
        IJobRunRepository jobRunRepo,
        IJobLongRunThresholdRepository thresholdRepo,
        IEventPublisher eventPublisher,
        ILogger<JobRunEventHandler> logger)
    {
        _jobRunRepo = jobRunRepo;
        _thresholdRepo = thresholdRepo;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task HandleAsync(string payload, CancellationToken ct)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(payload);
        var tenantId = data.GetProperty("tenantId").GetString() ?? string.Empty;
        var pipelineId = data.GetProperty("pipelineId").GetString() ?? string.Empty;
        var executionId = data.GetProperty("executionId").GetString() ?? string.Empty;
        var jobName = data.GetProperty("jobName").GetString() ?? string.Empty;
        var status = data.TryGetProperty("status", out var st) ? st.GetString() ?? string.Empty : string.Empty;

        var jobRun = new JobRun
        {
            Id = $"{executionId}_{jobName}",
            TenantId = tenantId,
            PipelineId = pipelineId,
            PipelineName = data.TryGetProperty("pipelineName", out var pn) ? pn.GetString() ?? string.Empty : string.Empty,
            ExecutionId = executionId,
            JobName = jobName,
            Status = status,
            StartTime = data.TryGetProperty("startTime", out var stm) ? stm.GetDateTimeOffset() : null,
            EndTime = data.TryGetProperty("endTime", out var etm) ? etm.GetDateTimeOffset() : null,
            DurationSeconds = data.TryGetProperty("durationSeconds", out var ds) ? ds.GetDouble() : null,
            ErrorMessage = data.TryGetProperty("errorMessage", out var em) ? em.GetString() : null,
            StackTrace = data.TryGetProperty("stackTrace", out var stk) ? stk.GetString() : null,
            RetryCount = data.TryGetProperty("retryCount", out var rc) ? rc.GetInt32() : 0,
            SourceSystemUrl = data.TryGetProperty("sourceSystemUrl", out var su) ? su.GetString() : null,
            HasGranularData = data.TryGetProperty("hasGranularData", out var hg) && hg.GetBoolean(),
            IsSkipped = data.TryGetProperty("isSkipped", out var sk) && sk.GetBoolean(),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        if (string.Equals(status, "running", StringComparison.OrdinalIgnoreCase) && jobRun.StartTime.HasValue)
        {
            var elapsed = (DateTimeOffset.UtcNow - jobRun.StartTime.Value).TotalSeconds;
            var threshold = await _thresholdRepo.GetByJobAsync(pipelineId, jobName, tenantId, ct).ConfigureAwait(false);
            if (threshold is { IsApplicable: true, ThresholdSeconds: not null } && elapsed > threshold.ThresholdSeconds.Value)
            {
                jobRun.IsLongRunning = true;
                var warningEvent = new NormalizedEvent
                {
                    EventType = "job.longrun.warning",
                    TenantId = tenantId,
                    Payload = JsonSerializer.Serialize(new { tenantId, pipelineId, executionId, jobName, elapsed, threshold = threshold.ThresholdSeconds }),
                };
                await _eventPublisher.PublishAsync("job-events", warningEvent, ct).ConfigureAwait(false);
                _logger.LogWarning("Long-running job detected: {JobName} in pipeline {PipelineId}, elapsed {Elapsed}s > threshold {Threshold}s",
                    jobName, pipelineId, elapsed, threshold.ThresholdSeconds);
            }
        }

        await _jobRunRepo.CreateAsync(jobRun, ct).ConfigureAwait(false);
        _logger.LogDebug("Processed job run {JobId} status={Status}", jobRun.Id, jobRun.Status);
    }
}

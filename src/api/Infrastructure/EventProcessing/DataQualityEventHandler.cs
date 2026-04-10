using System.Text.Json;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.EventProcessing;

public sealed class DataQualityEventHandler : IEventHandler
{
    public string EventType => "data.quality.score";

    private readonly IDataQualityScoreRepository _scoreRepo;
    private readonly IDataQualityStatusRepository _statusRepo;
    private readonly IDataQualityThresholdConfigRepository _configRepo;
    private readonly ILogger<DataQualityEventHandler> _logger;

    public DataQualityEventHandler(
        IDataQualityScoreRepository scoreRepo,
        IDataQualityStatusRepository statusRepo,
        IDataQualityThresholdConfigRepository configRepo,
        ILogger<DataQualityEventHandler> logger)
    {
        _scoreRepo = scoreRepo;
        _statusRepo = statusRepo;
        _configRepo = configRepo;
        _logger = logger;
    }

    public async Task HandleAsync(string payload, CancellationToken ct)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(payload);
        var tenantId = data.GetProperty("tenantId").GetString() ?? string.Empty;
        var pipelineId = data.GetProperty("pipelineId").GetString() ?? string.Empty;
        var overallScore = data.TryGetProperty("overallScore", out var os) ? os.GetDouble() : 0;

        var score = new DataQualityScore
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            PipelineId = pipelineId,
            PipelineName = data.TryGetProperty("pipelineName", out var pn) ? pn.GetString() ?? string.Empty : string.Empty,
            BusinessPlan = data.TryGetProperty("businessPlan", out var bp) ? bp.GetString() : null,
            OverallScore = overallScore,
            RunAt = data.TryGetProperty("runAt", out var ra) ? ra.GetDateTimeOffset() : DateTimeOffset.UtcNow,
            IngestedAt = DateTimeOffset.UtcNow,
        };

        if (data.TryGetProperty("checks", out var checksEl) && checksEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var c in checksEl.EnumerateArray())
            {
                score.Checks.Add(new QualityCheckResult
                {
                    CheckName = c.TryGetProperty("checkName", out var cn) ? cn.GetString() ?? string.Empty : string.Empty,
                    Passed = c.TryGetProperty("passed", out var p) && p.GetBoolean(),
                    RecordsEvaluated = c.TryGetProperty("recordsEvaluated", out var re) ? re.GetInt64() : 0,
                    RecordsFailed = c.TryGetProperty("recordsFailed", out var rf) ? rf.GetInt64() : 0,
                    FailureRate = c.TryGetProperty("failureRate", out var fr) ? fr.GetDouble() : 0,
                    Message = c.TryGetProperty("message", out var m) ? m.GetString() : null,
                });
            }
        }

        await _scoreRepo.CreateAsync(score, ct).ConfigureAwait(false);

        var config = await _configRepo.GetByPipelineIdAsync(pipelineId, tenantId, ct).ConfigureAwait(false);
        var qualityStatus = QualityStatus.NoData;
        if (config is not null)
        {
            qualityStatus = overallScore >= (config.WarningThreshold ?? 85) ? QualityStatus.Passing
                : overallScore >= (config.CriticalThreshold ?? 70) ? QualityStatus.Warning
                : QualityStatus.Failing;
        }

        var status = await _statusRepo.GetByPipelineIdAsync(pipelineId, tenantId, ct).ConfigureAwait(false)
            ?? new DataQualityStatus { Id = pipelineId, TenantId = tenantId, PipelineId = pipelineId };

        status.PipelineName = score.PipelineName;
        status.BusinessPlan = score.BusinessPlan;
        status.LatestScoreId = score.Id;
        status.QualityScore = overallScore;
        status.ScoreTimestamp = score.RunAt;
        status.WarningThreshold = config?.WarningThreshold;
        status.CriticalThreshold = config?.CriticalThreshold;
        status.QualityStatusValue = qualityStatus;
        status.EvaluatedAt = DateTimeOffset.UtcNow;

        await _statusRepo.UpsertAsync(status, ct).ConfigureAwait(false);
        _logger.LogDebug("Processed data quality score for {PipelineId}, qualityStatus={QualityStatus}", pipelineId, qualityStatus);
    }
}

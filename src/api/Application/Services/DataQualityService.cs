using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

public sealed class DataQualityService : IDataQualityService
{
    private readonly IDataQualityStatusRepository _statusRepo;
    private readonly IDataQualityScoreRepository _scoreRepo;
    private readonly IDataQualityThresholdConfigRepository _configRepo;

    public DataQualityService(IDataQualityStatusRepository statusRepo, IDataQualityScoreRepository scoreRepo, IDataQualityThresholdConfigRepository configRepo)
    {
        _statusRepo = statusRepo;
        _scoreRepo = scoreRepo;
        _configRepo = configRepo;
    }

    public async Task<List<DataQualityStatusDto>> GetStatusAsync(string tenantId, string? qualityStatus, CancellationToken ct)
    {
        var result = new List<DataQualityStatusDto>();
        await foreach (var s in _statusRepo.GetAllByTenantAsync(tenantId, ct))
        {
            var qs = s.QualityStatusValue.ToString();
            if (qualityStatus is not null && !string.Equals(qs, qualityStatus, StringComparison.OrdinalIgnoreCase)) continue;
            result.Add(new DataQualityStatusDto(s.PipelineId, s.PipelineName, s.BusinessPlan, s.Domain, s.QualityScore, s.ScoreTimestamp, qs, s.LatencyStatusValue.ToString(), s.LastSuccessfulRunAt, s.EvaluatedAt));
        }
        return result;
    }

    public async Task<List<DataQualityTrendPointDto>> GetTrendAsync(string tenantId, string pipelineId, int days, CancellationToken ct)
    {
        var endDate = DateTimeOffset.UtcNow;
        var startDate = endDate.AddDays(-days);
        var scores = new List<DataQualityScore>();
        await foreach (var s in _scoreRepo.GetByPipelineAndDateRangeAsync(pipelineId, tenantId, startDate, endDate, ct))
            scores.Add(s);

        var trend = new List<DataQualityTrendPointDto>();
        for (int i = 0; i < days; i++)
        {
            var date = endDate.AddDays(-days + i + 1).ToString("yyyy-MM-dd");
            var score = scores.FirstOrDefault(s => s.RunAt?.ToString("yyyy-MM-dd") == date);
            trend.Add(new DataQualityTrendPointDto(date, score?.OverallScore, score?.Id));
        }
        return trend;
    }

    public async Task<List<DataQualityCheckDto>?> GetChecksAsync(string tenantId, string pipelineId, string scoreId, CancellationToken ct)
    {
        var score = await _scoreRepo.GetByIdAsync(scoreId, tenantId, ct);
        if (score is null || score.PipelineId != pipelineId) return null;
        return score.Checks.Select(c => new DataQualityCheckDto(c.CheckName, c.Passed, c.RecordsEvaluated, c.RecordsFailed, c.FailureRate, c.Message)).ToList();
    }

    public async Task<bool> UpsertConfigAsync(string tenantId, string pipelineId, DataQualityThresholdRequest request, string userId, CancellationToken ct)
    {
        var existing = await _configRepo.GetByPipelineIdAsync(pipelineId, tenantId, ct);
        var isNew = existing is null;
        var config = existing ?? new DataQualityThresholdConfig { TenantId = tenantId, PipelineId = pipelineId };
        config.WarningThreshold = request.WarningThreshold;
        config.CriticalThreshold = request.CriticalThreshold;
        config.FreshnessThresholdSeconds = request.FreshnessThresholdSeconds;
        config.FreshnessBufferPercent = request.FreshnessBufferPercent;
        config.ConfiguredAt = DateTimeOffset.UtcNow;
        config.ConfiguredBy = userId;
        await _configRepo.UpsertAsync(config, ct);
        return isNew;
    }
}

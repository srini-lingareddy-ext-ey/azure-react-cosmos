namespace Todo.Api.Application.Transport;

public sealed record DataQualityStatusDto(string PipelineId, string PipelineName, string? BusinessPlan, string? Domain, double? QualityScore, DateTimeOffset? ScoreTimestamp, string QualityStatus, string LatencyStatus, DateTimeOffset? LastSuccessfulRunAt, DateTimeOffset? EvaluatedAt);
public sealed record DataQualityTrendPointDto(string Date, double? Score, string? ScoreId);
public sealed record DataQualityCheckDto(string CheckName, bool Passed, long RecordsEvaluated, long RecordsFailed, double FailureRate, string? Message);
public sealed record DataQualityThresholdRequest(double WarningThreshold, double CriticalThreshold, long? FreshnessThresholdSeconds, double? FreshnessBufferPercent);

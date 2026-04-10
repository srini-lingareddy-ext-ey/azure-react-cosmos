namespace Todo.Api.Application.Transport;

public sealed record JobRunDto(string JobName, string Status, DateTimeOffset? StartTime, DateTimeOffset? EndTime, double? DurationSeconds, int RetryCount, bool IsLongRunning, bool IsSkipped, bool HasGranularData, string? ErrorMessage, string? StackTrace, string? SourceSystemUrl);
public sealed record JobHistorySummaryDto(int TotalRuns, double SuccessRate, double? AverageDurationSeconds);
public sealed record JobHistoryResponse(JobHistorySummaryDto Summary, List<JobRunDto> History);

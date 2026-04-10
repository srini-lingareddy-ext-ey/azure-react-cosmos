namespace Todo.Api.Application.Transport;

public sealed record PipelineStatusDto(string PipelineId, string PipelineName, string? BusinessPlan, string? Domain, string? Layer, string Status, DateTimeOffset? LastRunAt, string? LatestExecutionId, List<HopSummaryDto> Hops);
public sealed record HopSummaryDto(string Layer, string Status, bool HasDetail);
public sealed record HopDetailDto(string Layer, string Status, DateTimeOffset? StartTime, DateTimeOffset? EndTime, double? DurationSeconds, string? ErrorMessage, string? SourceSystem);
public sealed record MemSQLInterfaceDto(string InterfaceName, string Status, long PendingRecordCount, DateTimeOffset? LastCompletedAt, string? LastErrorMessage);
public sealed record PipelineStatusListResponse(List<PipelineStatusDto> Data, PaginationDto Pagination);
public sealed record PaginationDto(int Total, bool HasMore);

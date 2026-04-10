namespace Todo.Api.Application.Transport;

public sealed record SLAStatusDto(string PipelineId, string PipelineName, string? BusinessPlan, string Status, double? TimeRemainingSeconds, string? SlaWindow, DateTimeOffset? LastRunAt, DateTimeOffset? EvaluatedAt);
public sealed record SLAComplianceSummaryDto(string BusinessPlan, double PercentageMet, int BreachCount);
public sealed record SLATrendPointDto(string Date, int Met, int Breached);
public sealed record SLAComplianceResponse(List<SLAComplianceSummaryDto> Summary, List<SLATrendPointDto> Trend, string? DataAvailabilityNote);
public sealed record SLABreachHistoryDto(string Id, DateTimeOffset BreachDetectedAt, DateTimeOffset SlaWindowClosedAt, DateTimeOffset? CompletedAt, int? MinutesOverdue);
public sealed record SLAConfigRequest(string WindowType, string WindowValue, int AtRiskBufferMinutes);

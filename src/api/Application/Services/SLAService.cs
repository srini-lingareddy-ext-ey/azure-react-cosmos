using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

public sealed class SLAService : ISLAService
{
    private readonly IPipelineSLAStatusRepository _statusRepo;
    private readonly IPipelineSLAConfigRepository _configRepo;
    private readonly IPipelineSLABreachRecordRepository _breachRepo;

    public SLAService(IPipelineSLAStatusRepository statusRepo, IPipelineSLAConfigRepository configRepo, IPipelineSLABreachRecordRepository breachRepo)
    {
        _statusRepo = statusRepo;
        _configRepo = configRepo;
        _breachRepo = breachRepo;
    }

    public async Task<List<SLAStatusDto>> GetStatusAsync(string tenantId, string? status, CancellationToken ct)
    {
        var result = new List<SLAStatusDto>();
        await foreach (var s in _statusRepo.GetAllByTenantAsync(tenantId, ct))
        {
            var statusStr = s.Status.ToString();
            if (status is not null && !string.Equals(statusStr, status, StringComparison.OrdinalIgnoreCase)) continue;
            result.Add(new SLAStatusDto(s.PipelineId, s.PipelineName, s.BusinessPlan, statusStr, s.TimeRemainingSeconds, null, s.LastRunAt, s.EvaluatedAt));
        }
        return result;
    }

    public async Task<SLAComplianceResponse> GetComplianceAsync(string tenantId, string? timeRange, CancellationToken ct)
    {
        var statuses = new List<PipelineSLAStatus>();
        await foreach (var s in _statusRepo.GetAllByTenantAsync(tenantId, ct)) statuses.Add(s);
        var summary = statuses.GroupBy(s => s.BusinessPlan ?? "Unassigned").Select(g => new SLAComplianceSummaryDto(g.Key,
            g.Count() > 0 ? Math.Round((double)g.Count(s => s.Status == SLAStatus.Met) / g.Count() * 100, 1) : 0,
            g.Count(s => s.Status == SLAStatus.Breached))).ToList();
        return new SLAComplianceResponse(summary, new List<SLATrendPointDto>(), null);
    }

    public async Task<List<SLABreachHistoryDto>> GetHistoryAsync(string tenantId, string pipelineId, int limit, CancellationToken ct)
    {
        var result = new List<SLABreachHistoryDto>();
        await foreach (var b in _breachRepo.GetByPipelineIdAsync(pipelineId, tenantId, limit, ct))
            result.Add(new SLABreachHistoryDto(b.Id, b.BreachDetectedAt, b.SlaWindowClosedAt, b.CompletedAt, b.MinutesOverdue));
        return result;
    }

    public async Task<bool> UpsertConfigAsync(string tenantId, string pipelineId, SLAConfigRequest request, string userId, CancellationToken ct)
    {
        var existing = await _configRepo.GetByPipelineIdAsync(pipelineId, tenantId, ct);
        var isNew = existing is null;
        var config = existing ?? new PipelineSLAConfig { TenantId = tenantId, PipelineId = pipelineId };
        config.WindowType = request.WindowType == "absoluteTime" ? SLAWindowType.AbsoluteTime : SLAWindowType.Duration;
        config.WindowValue = request.WindowValue;
        config.AtRiskBufferMinutes = request.AtRiskBufferMinutes;
        config.ConfiguredAt = DateTimeOffset.UtcNow;
        config.ConfiguredBy = userId;
        await _configRepo.UpsertAsync(config, ct);
        return isNew;
    }
}

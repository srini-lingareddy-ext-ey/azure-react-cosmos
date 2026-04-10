using Todo.Api.Application.Transport;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

public sealed class PipelineMonitoringService : IPipelineMonitoringService
{
    private readonly IPipelineStatusSummaryRepository _summaryRepo;
    private readonly IPipelineExecutionRepository _executionRepo;
    private readonly IMemSQLInterfaceStatusRepository _memsqlRepo;

    public PipelineMonitoringService(IPipelineStatusSummaryRepository summaryRepo, IPipelineExecutionRepository executionRepo, IMemSQLInterfaceStatusRepository memsqlRepo)
    {
        _summaryRepo = summaryRepo;
        _executionRepo = executionRepo;
        _memsqlRepo = memsqlRepo;
    }

    public async Task<PipelineStatusListResponse> GetStatusAsync(string tenantId, string? status, string? businessPlan, int limit, int offset, CancellationToken ct)
    {
        var all = new List<PipelineStatusDto>();
        await foreach (var s in _summaryRepo.GetAllByTenantAsync(tenantId, ct))
        {
            if (status is not null && !string.Equals(s.Status, status, StringComparison.OrdinalIgnoreCase)) continue;
            if (businessPlan is not null && !string.Equals(s.BusinessPlan, businessPlan, StringComparison.OrdinalIgnoreCase)) continue;
            all.Add(new PipelineStatusDto(s.PipelineId, s.PipelineName, s.BusinessPlan, s.Domain, s.Layer, s.Status, s.LastRunAt, s.LatestExecutionId,
                s.Hops.Select(h => new HopSummaryDto(h.Layer, h.Status, h.HasDetail)).ToList()));
        }
        var paged = all.Skip(offset).Take(limit).ToList();
        return new PipelineStatusListResponse(paged, new PaginationDto(all.Count, offset + limit < all.Count));
    }

    public async Task<HopDetailDto?> GetHopDetailAsync(string tenantId, string executionId, string layer, CancellationToken ct)
    {
        var exec = await _executionRepo.GetByIdAsync(executionId, tenantId, ct);
        if (exec is null) return null;
        var hop = exec.Hops.FirstOrDefault(h => string.Equals(h.Layer, layer, StringComparison.OrdinalIgnoreCase));
        if (hop is null) return null;
        return new HopDetailDto(hop.Layer, hop.Status, hop.StartTime, hop.EndTime, hop.DurationSeconds, hop.ErrorMessage, hop.SourceSystem);
    }

    public async Task<List<MemSQLInterfaceDto>> GetMemSQLInterfacesAsync(string tenantId, string? status, CancellationToken ct)
    {
        var result = new List<MemSQLInterfaceDto>();
        await foreach (var m in _memsqlRepo.GetAllByTenantAsync(tenantId, ct))
        {
            if (status is not null && !string.Equals(m.Status, status, StringComparison.OrdinalIgnoreCase)) continue;
            result.Add(new MemSQLInterfaceDto(m.InterfaceName, m.Status, m.PendingRecordCount, m.LastCompletedAt, m.LastErrorMessage));
        }
        return result;
    }
}

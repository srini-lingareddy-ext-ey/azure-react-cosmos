using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

public interface ISLAService
{
    Task<List<SLAStatusDto>> GetStatusAsync(string tenantId, string? status, CancellationToken ct);
    Task<SLAComplianceResponse> GetComplianceAsync(string tenantId, string? timeRange, CancellationToken ct);
    Task<List<SLABreachHistoryDto>> GetHistoryAsync(string tenantId, string pipelineId, int limit, CancellationToken ct);
    Task<bool> UpsertConfigAsync(string tenantId, string pipelineId, SLAConfigRequest request, string userId, CancellationToken ct);
}

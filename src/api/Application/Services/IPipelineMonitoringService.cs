using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

public interface IPipelineMonitoringService
{
    Task<PipelineStatusListResponse> GetStatusAsync(string tenantId, string? status, string? businessPlan, int limit, int offset, CancellationToken ct);
    Task<HopDetailDto?> GetHopDetailAsync(string tenantId, string executionId, string layer, CancellationToken ct);
    Task<List<MemSQLInterfaceDto>> GetMemSQLInterfacesAsync(string tenantId, string? status, CancellationToken ct);
}

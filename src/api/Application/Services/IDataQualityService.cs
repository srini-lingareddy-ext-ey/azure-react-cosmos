using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

public interface IDataQualityService
{
    Task<List<DataQualityStatusDto>> GetStatusAsync(string tenantId, string? qualityStatus, CancellationToken ct);
    Task<List<DataQualityTrendPointDto>> GetTrendAsync(string tenantId, string pipelineId, int days, CancellationToken ct);
    Task<List<DataQualityCheckDto>?> GetChecksAsync(string tenantId, string pipelineId, string scoreId, CancellationToken ct);
    Task<bool> UpsertConfigAsync(string tenantId, string pipelineId, DataQualityThresholdRequest request, string userId, CancellationToken ct);
}

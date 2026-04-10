using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

public interface IJobMonitoringService
{
    Task<List<JobRunDto>> GetJobsByExecutionAsync(string tenantId, string executionId, CancellationToken ct);
    Task<JobRunDto?> GetJobDetailAsync(string tenantId, string executionId, string jobName, CancellationToken ct);
    Task<JobHistoryResponse?> GetJobHistoryAsync(string tenantId, string pipelineId, string jobName, int days, CancellationToken ct);
}

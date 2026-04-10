using Todo.Api.Application.Transport;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

public sealed class JobMonitoringService : IJobMonitoringService
{
    private readonly IJobRunRepository _jobRunRepo;

    public JobMonitoringService(IJobRunRepository jobRunRepo) { _jobRunRepo = jobRunRepo; }

    public async Task<List<JobRunDto>> GetJobsByExecutionAsync(string tenantId, string executionId, CancellationToken ct)
    {
        var result = new List<JobRunDto>();
        await foreach (var r in _jobRunRepo.GetByExecutionIdAsync(executionId, tenantId, ct))
            result.Add(MapToDto(r));
        return result;
    }

    public async Task<JobRunDto?> GetJobDetailAsync(string tenantId, string executionId, string jobName, CancellationToken ct)
    {
        var id = $"{executionId}_{jobName}";
        var run = await _jobRunRepo.GetByIdAsync(id, tenantId, ct);
        return run is null ? null : MapToDto(run);
    }

    public async Task<JobHistoryResponse?> GetJobHistoryAsync(string tenantId, string pipelineId, string jobName, int days, CancellationToken ct)
    {
        var runs = new List<Domain.Entities.JobRun>();
        await foreach (var r in _jobRunRepo.GetByJobNameAndPipelineAsync(pipelineId, jobName, tenantId, days, ct))
            runs.Add(r);
        if (runs.Count == 0) return null;

        var successful = runs.Count(r => string.Equals(r.Status, "successful", StringComparison.OrdinalIgnoreCase));
        var successRate = runs.Count > 0 ? Math.Round((double)successful / runs.Count * 100, 1) : 0;
        var avgDuration = runs.Where(r => r.DurationSeconds.HasValue).Select(r => r.DurationSeconds!.Value).DefaultIfEmpty(0).Average();

        return new JobHistoryResponse(
            new JobHistorySummaryDto(runs.Count, successRate, avgDuration),
            runs.Select(MapToDto).ToList());
    }

    private static JobRunDto MapToDto(Domain.Entities.JobRun r) => new(r.JobName, r.Status, r.StartTime, r.EndTime, r.DurationSeconds, r.RetryCount, r.IsLongRunning, r.IsSkipped, r.HasGranularData, r.ErrorMessage, r.StackTrace, r.SourceSystemUrl);
}

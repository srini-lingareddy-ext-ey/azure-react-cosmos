using System.Text.Json;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.EventProcessing;

public sealed class PipelineExecutionEventHandler : IEventHandler
{
    public string EventType => "pipeline.execution";

    private readonly IPipelineExecutionRepository _executionRepo;
    private readonly IPipelineStatusSummaryRepository _summaryRepo;
    private readonly ILogger<PipelineExecutionEventHandler> _logger;

    public PipelineExecutionEventHandler(
        IPipelineExecutionRepository executionRepo,
        IPipelineStatusSummaryRepository summaryRepo,
        ILogger<PipelineExecutionEventHandler> logger)
    {
        _executionRepo = executionRepo;
        _summaryRepo = summaryRepo;
        _logger = logger;
    }

    public async Task HandleAsync(string payload, CancellationToken ct)
    {
        var data = JsonSerializer.Deserialize<JsonElement>(payload);
        var execution = new PipelineExecution
        {
            Id = data.GetProperty("executionId").GetString() ?? Guid.NewGuid().ToString(),
            TenantId = data.GetProperty("tenantId").GetString() ?? string.Empty,
            PipelineId = data.GetProperty("pipelineId").GetString() ?? string.Empty,
            PipelineName = data.TryGetProperty("pipelineName", out var pn) ? pn.GetString() ?? string.Empty : string.Empty,
            Status = data.TryGetProperty("status", out var st) ? st.GetString() ?? string.Empty : string.Empty,
            StartedAt = data.TryGetProperty("startedAt", out var sa) ? sa.GetDateTimeOffset() : DateTimeOffset.UtcNow,
            CompletedAt = data.TryGetProperty("completedAt", out var ca) ? ca.GetDateTimeOffset() : null,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        if (data.TryGetProperty("hops", out var hopsEl) && hopsEl.ValueKind == JsonValueKind.Array)
        {
            foreach (var h in hopsEl.EnumerateArray())
            {
                execution.Hops.Add(new HopDetail
                {
                    Layer = h.TryGetProperty("layer", out var l) ? l.GetString() ?? string.Empty : string.Empty,
                    Status = h.TryGetProperty("status", out var hs) ? hs.GetString() ?? string.Empty : string.Empty,
                    StartTime = h.TryGetProperty("startTime", out var hst) ? hst.GetDateTimeOffset() : null,
                    EndTime = h.TryGetProperty("endTime", out var het) ? het.GetDateTimeOffset() : null,
                    DurationSeconds = h.TryGetProperty("durationSeconds", out var hds) ? hds.GetDouble() : null,
                    ErrorMessage = h.TryGetProperty("errorMessage", out var hem) ? hem.GetString() : null,
                    SourceSystem = h.TryGetProperty("sourceSystem", out var hss) ? hss.GetString() : null,
                });
            }
        }

        await _executionRepo.CreateAsync(execution, ct).ConfigureAwait(false);

        var summary = await _summaryRepo.GetByIdAsync(execution.PipelineId, execution.TenantId, ct).ConfigureAwait(false)
            ?? new PipelineStatusSummary { Id = execution.PipelineId, TenantId = execution.TenantId, PipelineId = execution.PipelineId };

        summary.PipelineName = execution.PipelineName;
        summary.Status = execution.Status;
        summary.LatestExecutionId = execution.Id;
        summary.LastRunAt = execution.StartedAt;
        summary.UpdatedAt = DateTimeOffset.UtcNow;
        summary.Hops = execution.Hops.Select(h => new HopSummary { Layer = h.Layer, Status = h.Status, HasDetail = true }).ToList();

        await _summaryRepo.UpsertAsync(summary, ct).ConfigureAwait(false);
        _logger.LogDebug("Processed pipeline execution {ExecutionId} for {PipelineId}", execution.Id, execution.PipelineId);
    }
}

using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Monitor = Todo.Api.Domain.Entities.Monitor;

namespace Todo.Api.Infrastructure.EventProcessing;

public sealed class EventEnrichmentService
{
    private readonly IMonitorRepository _monitorRepo;
    private readonly ILogger<EventEnrichmentService> _logger;

    public EventEnrichmentService(IMonitorRepository monitorRepo, ILogger<EventEnrichmentService> logger)
    {
        _monitorRepo = monitorRepo;
        _logger = logger;
    }

    public async Task EnrichAsync(Event evt, string connectorId, string monitorId, string tenantId, CancellationToken ct)
    {
        evt.TenantId = tenantId;
        evt.ConnectorId = connectorId;
        evt.MonitorId = monitorId;

        if (!string.IsNullOrEmpty(monitorId) && !string.IsNullOrEmpty(tenantId))
        {
            var monitor = await _monitorRepo.GetByIdAsync(monitorId, tenantId, ct).ConfigureAwait(false);
            if (monitor is not null)
            {
                evt.MonitorName = monitor.MonitorName;
                evt.BusinessPlan = monitor.BusinessPlanId;
                evt.PipelineId = monitor.EntityId;
            }
            else
            {
                evt.MonitorName = "unknown";
                _logger.LogWarning("Monitor {MonitorId} not found for enrichment in tenant {TenantId}", monitorId, tenantId);
            }
        }

        evt.EnrichedAt = DateTimeOffset.UtcNow;
    }
}

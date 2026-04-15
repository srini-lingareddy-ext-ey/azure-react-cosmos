using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Todo.Api.Application.Services;

namespace Todo.Api.Infrastructure.BackgroundJobs;

/// <summary>WO-68: escalates open incidents if threshold exceeded (5-min cycle).</summary>
public sealed class IncidentEscalationJob : BackgroundService
{
    private static readonly TimeSpan EscalationThreshold = TimeSpan.FromHours(4);
    private static readonly Dictionary<IncidentSeverity, IncidentSeverity> EscalationMap = new()
    {
        [IncidentSeverity.Low] = IncidentSeverity.Medium,
        [IncidentSeverity.Medium] = IncidentSeverity.High,
        [IncidentSeverity.High] = IncidentSeverity.Critical,
    };

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<IncidentEscalationJob> _logger;

    public IncidentEscalationJob(IServiceProvider serviceProvider, ILogger<IncidentEscalationJob> logger)
    { _serviceProvider = serviceProvider; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        _logger.LogInformation("IncidentEscalationJob started (5-minute cycle)");
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await EvaluateAllAsync(scope.ServiceProvider, stoppingToken).ConfigureAwait(false);
                _logger.LogDebug("Incident escalation cycle completed");
            }
            catch (Exception ex) when (ex is not OperationCanceledException) { _logger.LogError(ex, "Incident escalation cycle failed"); }
        }
    }

    private async Task EvaluateAllAsync(IServiceProvider sp, CancellationToken ct)
    {
        var tenantRepo = sp.GetRequiredService<ITenantRepository>();
        var incidentRepo = sp.GetRequiredService<IIncidentRepository>();
        var notificationService = sp.GetRequiredService<INotificationDeliveryService>();
        await foreach (var tenant in tenantRepo.GetAllAsync(ct).ConfigureAwait(false))
        {
            await foreach (var incident in incidentRepo.GetByTenantAsync(tenant.Id, null, null, null, null, null, null, 1000, 0, ct).ConfigureAwait(false))
            {
                if (incident.State != IncidentState.Open) continue;
                if (!incident.CreatedAt.HasValue) continue;
                if ((DateTimeOffset.UtcNow - incident.CreatedAt.Value) < EscalationThreshold) continue;
                if (incident.Severity == IncidentSeverity.Critical) continue;
                if (!EscalationMap.TryGetValue(incident.Severity, out var newSeverity)) continue;

                var oldSeverity = incident.Severity;
                incident.Severity = newSeverity;
                incident.UpdatedAt = DateTimeOffset.UtcNow;
                incident.UpdatedBy = "system:escalation";
                incident.StateHistory.Add(new StateHistoryEntry { FromState = incident.State.ToString(), ToState = incident.State.ToString(), Actor = "system:escalation", Timestamp = DateTimeOffset.UtcNow, Note = $"Auto-escalated severity from {oldSeverity} to {newSeverity} (threshold: {EscalationThreshold.TotalHours}h)" });
                await incidentRepo.UpdateAsync(incident, ct).ConfigureAwait(false);
                _logger.LogInformation("Escalated incident {IncidentId} from {Old} to {New}", incident.Id, oldSeverity, newSeverity);
                try { await notificationService.DeliverEscalationAsync(incident.Id, tenant.Id, ct).ConfigureAwait(false); }
                catch (Exception ex) { _logger.LogWarning(ex, "Escalation notification failed for incident {IncidentId}", incident.Id); }
            }
        }
    }
}

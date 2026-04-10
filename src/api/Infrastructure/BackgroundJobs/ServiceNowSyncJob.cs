using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;
using Todo.Api.Infrastructure.Integrations;

namespace Todo.Api.Infrastructure.BackgroundJobs;

/// <summary>WO-68: syncs incidents with ServiceNow every 5 minutes.</summary>
public sealed class ServiceNowSyncJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ServiceNowSyncJob> _logger;

    public ServiceNowSyncJob(IServiceProvider serviceProvider, ILogger<ServiceNowSyncJob> logger)
    { _serviceProvider = serviceProvider; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));
        _logger.LogInformation("ServiceNowSyncJob started (5-minute cycle)");
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                await SyncAllTenantsAsync(scope.ServiceProvider, stoppingToken).ConfigureAwait(false);
                _logger.LogDebug("ServiceNow sync cycle completed");
            }
            catch (Exception ex) when (ex is not OperationCanceledException) { _logger.LogError(ex, "ServiceNow sync cycle failed"); }
        }
    }

    private async Task SyncAllTenantsAsync(IServiceProvider sp, CancellationToken ct)
    {
        var tenantRepo = sp.GetRequiredService<ITenantRepository>();
        var incidentRepo = sp.GetRequiredService<IIncidentRepository>();
        var snConfigRepo = sp.GetRequiredService<IServiceNowConfigRepository>();
        var snClient = sp.GetRequiredService<IServiceNowClient>();
        await foreach (var tenant in tenantRepo.GetAllAsync(ct).ConfigureAwait(false))
        {
            var config = await snConfigRepo.GetByTenantIdAsync(tenant.Id, ct).ConfigureAwait(false);
            if (config is null) continue;
            try { await SyncTenantAsync(incidentRepo, snClient, tenant.Id, ct).ConfigureAwait(false); }
            catch (Exception ex) { _logger.LogError(ex, "ServiceNow sync failed for tenant {TenantId}", tenant.Id); }
        }
    }

    private async Task SyncTenantAsync(IIncidentRepository incidentRepo, IServiceNowClient snClient, string tenantId, CancellationToken ct)
    {
        await foreach (var incident in incidentRepo.GetByTenantAsync(tenantId, null, null, null, null, null, null, 1000, 0, ct).ConfigureAwait(false))
        {
            if (string.IsNullOrEmpty(incident.ServiceNowTicketNumber)) continue;
            foreach (var note in incident.Notes.Where(n => !n.SyncedToServiceNow))
            {
                try { await snClient.AddWorkNoteAsync(incident.ServiceNowTicketNumber, note.Content, ct).ConfigureAwait(false); note.SyncedToServiceNow = true; }
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to sync note {NoteId} to ServiceNow", note.NoteId); }
            }
            var snTicket = await snClient.GetTicketAsync(incident.ServiceNowTicketNumber, ct).ConfigureAwait(false);
            if (snTicket is not null) { incident.ServiceNowTicketStatus = snTicket.State; incident.LastSyncedAt = DateTimeOffset.UtcNow; }
            incident.UpdatedAt = DateTimeOffset.UtcNow;
            await incidentRepo.UpdateAsync(incident, ct).ConfigureAwait(false);
        }
    }
}

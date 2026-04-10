using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.BackgroundJobs;

public sealed class EventRetentionPurgeJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EventRetentionPurgeJob> _logger;

    public EventRetentionPurgeJob(IServiceProvider serviceProvider, ILogger<EventRetentionPurgeJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EventRetentionPurgeJob started (nightly at 03:00 UTC)");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(3);
            var delay = nextRun - now;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken).ConfigureAwait(false);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var tenantRepo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
                var eventRepo = scope.ServiceProvider.GetRequiredService<IEventRepository>();
                var auditRepo = scope.ServiceProvider.GetRequiredService<IRepository<PurgeAuditEntry>>();

                await foreach (var tenant in tenantRepo.GetAllAsync(stoppingToken).ConfigureAwait(false))
                {
                    var retentionDays = 90;
                    var cutoff = DateTimeOffset.UtcNow.AddDays(-retentionDays);
                    try
                    {
                        var deleted = await eventRepo.DeleteOlderThanAsync(tenant.Id, cutoff, stoppingToken).ConfigureAwait(false);
                        if (deleted > 0)
                        {
                            var purgeEntry = new PurgeAuditEntry
                            {
                                Id = Guid.NewGuid().ToString(),
                                TenantId = tenant.Id,
                                PurgedAt = DateTimeOffset.UtcNow,
                                DeletedCount = deleted,
                                RetentionDaysApplied = retentionDays,
                            };
                            await auditRepo.CreateAsync(purgeEntry, stoppingToken).ConfigureAwait(false);
                            _logger.LogInformation("Purged {Count} events for tenant {TenantId} (retention={Days}d)", deleted, tenant.Id, retentionDays);
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Purge failed for tenant {TenantId}", tenant.Id);
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "EventRetentionPurgeJob cycle failed");
            }
        }
    }
}

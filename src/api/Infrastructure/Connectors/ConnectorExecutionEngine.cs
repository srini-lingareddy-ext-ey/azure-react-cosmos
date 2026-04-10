using Cronos;
using Todo.Api.Application.EventPublishing;
using Todo.Api.Application.Connectors;
using Todo.Api.Application.Services;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Connectors;

/// <summary>WO-48: background service that polls enabled connectors on cron schedules.</summary>
public sealed class ConnectorExecutionEngine : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ConnectorExecutionEngine> _logger;

    public ConnectorExecutionEngine(IServiceScopeFactory scopeFactory, ILogger<ConnectorExecutionEngine> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
        while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                await RunCycleAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "ConnectorExecutionEngine: unhandled error in cycle");
            }
        }
    }

    private async Task RunCycleAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var connectorRepo = scope.ServiceProvider.GetRequiredService<IConnectorInstanceRepository>();
        var logRepo = scope.ServiceProvider.GetRequiredService<IConnectorExecutionLogRepository>();
        var encryptionService = scope.ServiceProvider.GetRequiredService<ICredentialEncryptionService>();
        var adapters = scope.ServiceProvider.GetServices<IConnectorAdapter>().ToDictionary(a => a.ConnectorTypeId, StringComparer.OrdinalIgnoreCase);
        var publisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();
        var healthTracker = scope.ServiceProvider.GetRequiredService<ConnectorHealthTracker>();
        var now = DateTimeOffset.UtcNow;

        // Iterate all tenants' enabled connectors (simplified: get all, filter enabled polling)
        // In a production system this would be per-tenant sharded
        var tasks = new List<Task>();

        // We need to load connectors across tenants; for MVP we use a known pattern
        // The engine loads all enabled polling connectors
        await foreach (var connector in GetAllEnabledPollingConnectorsAsync(connectorRepo, cancellationToken).ConfigureAwait(false))
        {
            if (string.IsNullOrEmpty(connector.PollingScheduleCron)) continue;

            try
            {
                var cron = CronExpression.Parse(connector.PollingScheduleCron);
                var nextOccurrence = cron.GetNextOccurrence(now.AddMinutes(-1).UtcDateTime, TimeZoneInfo.Utc);
                if (nextOccurrence is null || nextOccurrence > now.UtcDateTime) continue;
            }
            catch
            {
                _logger.LogWarning("Invalid cron expression for connector {Id}: {Cron}", connector.Id, connector.PollingScheduleCron);
                continue;
            }

            var c = connector;
            tasks.Add(Task.Run(async () =>
            {
                await ExecuteConnectorAsync(c, adapters, encryptionService, publisher, healthTracker, logRepo, cancellationToken).ConfigureAwait(false);
            }, cancellationToken));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task ExecuteConnectorAsync(
        ConnectorInstance connector,
        Dictionary<string, IConnectorAdapter> adapters,
        ICredentialEncryptionService encryptionService,
        IEventPublisher publisher,
        ConnectorHealthTracker healthTracker,
        IConnectorExecutionLogRepository logRepo,
        CancellationToken cancellationToken)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            if (!adapters.TryGetValue(connector.ConnectorTypeId, out var adapter))
            {
                _logger.LogWarning("No adapter found for connector type {Type}", connector.ConnectorTypeId);
                return;
            }

            var decrypted = await encryptionService.DecryptAsync(connector.CredentialsEncrypted, connector.TenantId).ConfigureAwait(false);
            var events = await adapter.PollAsync(decrypted, cancellationToken).ConfigureAwait(false);

            foreach (var evt in events)
            {
                evt.ConnectorId = connector.Id;
                evt.TenantId = connector.TenantId;
                var hubName = ResolveHubName(connector.ConnectorTypeId);
                await publisher.PublishAsync(hubName, evt, cancellationToken).ConfigureAwait(false);
            }

            sw.Stop();
            await logRepo.CreateAsync(new ConnectorExecutionLog
            {
                Id = Guid.NewGuid().ToString("n"),
                TenantId = connector.TenantId,
                ConnectorId = connector.Id,
                ExecutedAt = DateTimeOffset.UtcNow,
                Status = ExecutionStatus.Success,
                EventsProduced = events.Count,
                DurationMs = sw.ElapsedMilliseconds,
            }, cancellationToken).ConfigureAwait(false);

            await healthTracker.RecordSuccessAsync(connector.Id, connector.TenantId, events.Count, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            sw.Stop();
            _logger.LogError(ex, "Connector {Id} polling failed", connector.Id);

            try
            {
                await logRepo.CreateAsync(new ConnectorExecutionLog
                {
                    Id = Guid.NewGuid().ToString("n"),
                    TenantId = connector.TenantId,
                    ConnectorId = connector.Id,
                    ExecutedAt = DateTimeOffset.UtcNow,
                    Status = ExecutionStatus.Failed,
                    EventsProduced = 0,
                    DurationMs = sw.ElapsedMilliseconds,
                    ErrorMessage = ex.Message,
                }, cancellationToken).ConfigureAwait(false);

                await healthTracker.RecordFailureAsync(connector.Id, connector.TenantId, ex.Message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception inner)
            {
                _logger.LogError(inner, "Failed to record execution log for connector {Id}", connector.Id);
            }
        }
    }

    private static string ResolveHubName(string connectorTypeId) =>
        connectorTypeId switch
        {
            "datadog" or "newrelic" or "dynatrace" => "infrastructure-events",
            _ => "pipeline-events",
        };

    private static async IAsyncEnumerable<ConnectorInstance> GetAllEnabledPollingConnectorsAsync(
        IConnectorInstanceRepository repo,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // MVP: iterate a known set of tenants or all documents
        // For now, we use a broad scan. In production this would be optimized.
        await foreach (var c in repo.GetAllEnabledAsync(string.Empty, cancellationToken).ConfigureAwait(false))
        {
            if (c.IntegrationMode == IntegrationMode.Polling)
                yield return c;
        }
    }
}


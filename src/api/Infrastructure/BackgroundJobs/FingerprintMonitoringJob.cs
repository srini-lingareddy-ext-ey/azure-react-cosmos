using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Todo.Api.Application.EventPublishing;
using Todo.Api.Application.Services;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.BackgroundJobs;

public sealed class FingerprintMonitoringJob : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FingerprintMonitoringJob> _logger;

    public FingerprintMonitoringJob(IServiceProvider serviceProvider, ILogger<FingerprintMonitoringJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("FingerprintMonitoringJob started (nightly at 02:00 UTC)");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var nextRun = now.Date.AddDays(1).AddHours(2);
            var delay = nextRun - now;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay, stoppingToken).ConfigureAwait(false);

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var tenantRepo = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
                var artifactRepo = scope.ServiceProvider.GetRequiredService<IMonitoredArtifactRepository>();
                var fingerprintRepo = scope.ServiceProvider.GetRequiredService<IArtifactFingerprintRepository>();
                var windowRepo = scope.ServiceProvider.GetRequiredService<IFingerprintApprovedWindowRepository>();
                var auditRepo = scope.ServiceProvider.GetRequiredService<IFingerprintAuditRepository>();
                var retrievalService = scope.ServiceProvider.GetRequiredService<IArtifactRetrievalService>();
                var eventPublisher = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

                await foreach (var tenant in tenantRepo.GetAllAsync(stoppingToken).ConfigureAwait(false))
                {
                    await foreach (var artifact in artifactRepo.GetAllActiveByTenantAsync(tenant.Id, stoppingToken).ConfigureAwait(false))
                    {
                        await ScanArtifactAsync(artifact, tenant.Id, fingerprintRepo, windowRepo, auditRepo, artifactRepo, retrievalService, eventPublisher, stoppingToken).ConfigureAwait(false);
                    }
                }

                _logger.LogInformation("FingerprintMonitoringJob cycle completed");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "FingerprintMonitoringJob cycle failed");
            }
        }
    }

    private async Task ScanArtifactAsync(
        MonitoredArtifact artifact, string tenantId,
        IArtifactFingerprintRepository fingerprintRepo,
        IFingerprintApprovedWindowRepository windowRepo,
        IFingerprintAuditRepository auditRepo,
        IMonitoredArtifactRepository artifactRepo,
        IArtifactRetrievalService retrievalService,
        IEventPublisher eventPublisher,
        CancellationToken ct)
    {
        string currentHash;
        try
        {
            var content = await retrievalService.RetrieveCanonicalContentAsync(artifact, ct).ConfigureAwait(false);
            currentHash = $"sha256:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content))).ToLowerInvariant()}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve artifact {ArtifactId} content", artifact.Id);
            artifact.CurrentStatus = ArtifactStatus.Unknown;
            artifact.LastScannedAt = DateTimeOffset.UtcNow;
            await artifactRepo.UpdateAsync(artifact, ct).ConfigureAwait(false);
            return;
        }

        artifact.LastScannedAt = DateTimeOffset.UtcNow;

        var fingerprint = await fingerprintRepo.GetByArtifactIdAsync(artifact.Id, tenantId, ct).ConfigureAwait(false);
        if (fingerprint is null)
        {
            fingerprint = new ArtifactFingerprint
            {
                Id = artifact.Id,
                TenantId = tenantId,
                ArtifactId = artifact.Id,
                ApprovedHash = currentHash,
                ApprovedAt = DateTimeOffset.UtcNow,
                IsInitialBaseline = true,
            };
            await fingerprintRepo.UpsertAsync(fingerprint, ct).ConfigureAwait(false);
            artifact.CurrentStatus = ArtifactStatus.Baseline;
            await artifactRepo.UpdateAsync(artifact, ct).ConfigureAwait(false);
            return;
        }

        if (string.Equals(currentHash, fingerprint.ApprovedHash, StringComparison.OrdinalIgnoreCase))
        {
            artifact.CurrentStatus = ArtifactStatus.Baseline;
            await artifactRepo.UpdateAsync(artifact, ct).ConfigureAwait(false);
            return;
        }

        var activeWindows = new List<FingerprintApprovedWindow>();
        await foreach (var w in windowRepo.GetActiveWindowsForArtifactAsync(artifact.Id, artifact.ArtifactType, tenantId, DateTimeOffset.UtcNow, ct).ConfigureAwait(false))
            activeWindows.Add(w);

        var isAuthorized = activeWindows.Count > 0;
        var auditEntry = new FingerprintAuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            ArtifactId = artifact.Id,
            ArtifactName = artifact.ArtifactName,
            ArtifactType = artifact.ArtifactType,
            DetectedAt = DateTimeOffset.UtcNow,
            BeforeHash = fingerprint.ApprovedHash,
            AfterHash = currentHash,
            ChangeClassification = isAuthorized ? ChangeClassification.Authorized : ChangeClassification.Unauthorized,
            ApprovedWindowId = activeWindows.FirstOrDefault()?.Id,
            ApprovedWindowName = activeWindows.FirstOrDefault()?.Name,
        };
        await auditRepo.CreateAsync(auditEntry, ct).ConfigureAwait(false);

        if (isAuthorized)
        {
            fingerprint.ApprovedHash = currentHash;
            fingerprint.ApprovedAt = DateTimeOffset.UtcNow;
            fingerprint.IsInitialBaseline = false;
            await fingerprintRepo.UpsertAsync(fingerprint, ct).ConfigureAwait(false);
            artifact.CurrentStatus = ArtifactStatus.Baseline;
        }
        else
        {
            artifact.CurrentStatus = ArtifactStatus.Deviated;
            artifact.LastDeviationDetectedAt = DateTimeOffset.UtcNow;

            var alertEvt = new NormalizedEvent
            {
                EventType = "fingerprint.unauthorized.change",
                TenantId = tenantId,
                Payload = JsonSerializer.Serialize(new { artifactId = artifact.Id, artifactName = artifact.ArtifactName, beforeHash = fingerprint.ApprovedHash, afterHash = currentHash }),
            };
            await eventPublisher.PublishAsync("InfrastructureEvents", alertEvt, ct).ConfigureAwait(false);
        }

        await artifactRepo.UpdateAsync(artifact, ct).ConfigureAwait(false);
    }
}

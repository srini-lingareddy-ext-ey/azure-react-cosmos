using System.Security.Cryptography;
using System.Text;
using Todo.Api.Application.Transport;
using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Application.Services;

public sealed class FingerprintingService : IFingerprintingService
{
    private readonly IMonitoredArtifactRepository _artifactRepo;
    private readonly IArtifactFingerprintRepository _fingerprintRepo;
    private readonly IFingerprintApprovedWindowRepository _windowRepo;
    private readonly IFingerprintAuditRepository _auditRepo;
    private readonly IArtifactRetrievalService _retrievalService;

    public FingerprintingService(
        IMonitoredArtifactRepository artifactRepo,
        IArtifactFingerprintRepository fingerprintRepo,
        IFingerprintApprovedWindowRepository windowRepo,
        IFingerprintAuditRepository auditRepo,
        IArtifactRetrievalService retrievalService)
    {
        _artifactRepo = artifactRepo;
        _fingerprintRepo = fingerprintRepo;
        _windowRepo = windowRepo;
        _auditRepo = auditRepo;
        _retrievalService = retrievalService;
    }

    public async Task<List<MonitoredArtifactView>> GetArtifactsAsync(string tenantId, CancellationToken ct)
    {
        var list = new List<MonitoredArtifactView>();
        await foreach (var a in _artifactRepo.GetAllByTenantAsync(tenantId, ct).ConfigureAwait(false))
            list.Add(MapArtifact(a));
        return list;
    }

    public async Task<MonitoredArtifactView> RegisterArtifactAsync(string tenantId, string userId, RegisterArtifactRequest request, CancellationToken ct)
    {
        var artifact = new MonitoredArtifact
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            ArtifactName = request.ArtifactName,
            ArtifactType = Enum.Parse<ArtifactType>(request.ArtifactType, true),
            ConnectorId = request.ConnectorId,
            RetrievalConfig = request.RetrievalConfig,
            ScanScheduleCron = request.ScanScheduleCron ?? "0 2 * * *",
            CurrentStatus = ArtifactStatus.Unknown,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
        };
        await _artifactRepo.CreateAsync(artifact, ct).ConfigureAwait(false);
        return MapArtifact(artifact);
    }

    public async Task TriggerScanAsync(string artifactId, string tenantId, CancellationToken ct)
    {
        var artifact = await _artifactRepo.GetByIdAsync(artifactId, tenantId, ct).ConfigureAwait(false);
        if (artifact is null) throw new KeyNotFoundException($"Artifact {artifactId} not found");
    }

    public async Task ResetBaselineAsync(string artifactId, string tenantId, string userId, string justification, CancellationToken ct)
    {
        var artifact = await _artifactRepo.GetByIdAsync(artifactId, tenantId, ct).ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Artifact {artifactId} not found");

        var content = await _retrievalService.RetrieveCanonicalContentAsync(artifact, ct).ConfigureAwait(false);
        var hash = $"sha256:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(content))).ToLowerInvariant()}";

        var fingerprint = await _fingerprintRepo.GetByArtifactIdAsync(artifactId, tenantId, ct).ConfigureAwait(false);
        var previousHash = fingerprint?.ApprovedHash ?? string.Empty;

        var fp = fingerprint ?? new ArtifactFingerprint { Id = artifactId, TenantId = tenantId, ArtifactId = artifactId };
        fp.ApprovedHash = hash;
        fp.ApprovedAt = DateTimeOffset.UtcNow;
        fp.ApprovedBy = userId;
        fp.IsInitialBaseline = false;
        await _fingerprintRepo.UpsertAsync(fp, ct).ConfigureAwait(false);

        artifact.CurrentStatus = ArtifactStatus.Baseline;
        artifact.UpdatedAt = DateTimeOffset.UtcNow;
        await _artifactRepo.UpdateAsync(artifact, ct).ConfigureAwait(false);

        var auditEntry = new FingerprintAuditEntry
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            ArtifactId = artifactId,
            ArtifactName = artifact.ArtifactName,
            ArtifactType = artifact.ArtifactType,
            DetectedAt = DateTimeOffset.UtcNow,
            ChangedBy = userId,
            BeforeHash = previousHash,
            AfterHash = hash,
            ChangeClassification = ChangeClassification.Authorized,
            BaselineResetJustification = justification,
        };
        await _auditRepo.CreateAsync(auditEntry, ct).ConfigureAwait(false);
    }

    public async Task<List<FingerprintAuditEntryView>> GetAuditTrailAsync(string tenantId, string? changeClassification, int limit, int offset, CancellationToken ct)
    {
        var list = new List<FingerprintAuditEntryView>();
        await foreach (var e in _auditRepo.GetByTenantAsync(tenantId, changeClassification, limit, offset, ct).ConfigureAwait(false))
        {
            list.Add(new FingerprintAuditEntryView
            {
                Id = e.Id, ArtifactId = e.ArtifactId, ArtifactName = e.ArtifactName,
                ArtifactType = e.ArtifactType.ToString(), DetectedAt = e.DetectedAt, ChangedBy = e.ChangedBy,
                BeforeHash = e.BeforeHash, AfterHash = e.AfterHash,
                ChangeClassification = e.ChangeClassification.ToString(),
                ApprovedWindowName = e.ApprovedWindowName, SyncedToImmutableStorage = e.SyncedToImmutableStorage,
            });
        }
        return list;
    }

    public async Task<List<ApprovedWindowView>> GetWindowsAsync(string tenantId, CancellationToken ct)
    {
        var list = new List<ApprovedWindowView>();
        await foreach (var w in _windowRepo.GetAllByTenantAsync(tenantId, ct).ConfigureAwait(false))
            list.Add(new ApprovedWindowView { Id = w.Id, Name = w.Name, StartTime = w.StartTime, EndTime = w.EndTime, ScopeType = w.ScopeType.ToString(), ScopeValue = w.ScopeValue });
        return list;
    }

    public async Task<ApprovedWindowView> CreateWindowAsync(string tenantId, string userId, CreateApprovedWindowRequest request, CancellationToken ct)
    {
        var window = new FingerprintApprovedWindow
        {
            Id = Guid.NewGuid().ToString(),
            TenantId = tenantId,
            Name = request.Name,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            ScopeType = Enum.Parse<WindowScopeType>(request.ScopeType, true),
            ScopeValue = request.ScopeValue,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = userId,
        };
        await _windowRepo.CreateAsync(window, ct).ConfigureAwait(false);
        return new ApprovedWindowView { Id = window.Id, Name = window.Name, StartTime = window.StartTime, EndTime = window.EndTime, ScopeType = window.ScopeType.ToString(), ScopeValue = window.ScopeValue };
    }

    public Task DeleteWindowAsync(string windowId, string tenantId, CancellationToken ct) =>
        _windowRepo.DeleteAsync(windowId, tenantId, ct);

    private static MonitoredArtifactView MapArtifact(MonitoredArtifact a) => new()
    {
        ArtifactId = a.Id, ArtifactName = a.ArtifactName, ArtifactType = a.ArtifactType.ToString(),
        CurrentStatus = a.CurrentStatus.ToString(), LastScannedAt = a.LastScannedAt, LastDeviationDetectedAt = a.LastDeviationDetectedAt,
    };
}

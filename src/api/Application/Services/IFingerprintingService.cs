using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

public interface IFingerprintingService
{
    Task<List<MonitoredArtifactView>> GetArtifactsAsync(string tenantId, CancellationToken ct);
    Task<MonitoredArtifactView> RegisterArtifactAsync(string tenantId, string userId, RegisterArtifactRequest request, CancellationToken ct);
    Task TriggerScanAsync(string artifactId, string tenantId, CancellationToken ct);
    Task ResetBaselineAsync(string artifactId, string tenantId, string userId, string justification, CancellationToken ct);
    Task<List<FingerprintAuditEntryView>> GetAuditTrailAsync(string tenantId, string? changeClassification, int limit, int offset, CancellationToken ct);
    Task<List<ApprovedWindowView>> GetWindowsAsync(string tenantId, CancellationToken ct);
    Task<ApprovedWindowView> CreateWindowAsync(string tenantId, string userId, CreateApprovedWindowRequest request, CancellationToken ct);
    Task DeleteWindowAsync(string windowId, string tenantId, CancellationToken ct);
}

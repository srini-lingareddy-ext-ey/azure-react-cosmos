using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IFingerprintAuditRepository
{
    Task<FingerprintAuditEntry> CreateAsync(FingerprintAuditEntry entry, CancellationToken cancellationToken = default);
    IAsyncEnumerable<FingerprintAuditEntry> GetByTenantAsync(string tenantId, string? changeClassification, int limit = 50, int offset = 0, CancellationToken cancellationToken = default);
    IAsyncEnumerable<FingerprintAuditEntry> GetByArtifactIdAsync(string artifactId, string tenantId, int limit = 30, CancellationToken cancellationToken = default);
}

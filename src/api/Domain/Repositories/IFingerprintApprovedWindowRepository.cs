using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IFingerprintApprovedWindowRepository
{
    IAsyncEnumerable<FingerprintApprovedWindow> GetActiveWindowsForArtifactAsync(string artifactId, ArtifactType artifactType, string tenantId, DateTimeOffset at, CancellationToken cancellationToken = default);
    IAsyncEnumerable<FingerprintApprovedWindow> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<FingerprintApprovedWindow> CreateAsync(FingerprintApprovedWindow entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, string tenantId, CancellationToken cancellationToken = default);
}

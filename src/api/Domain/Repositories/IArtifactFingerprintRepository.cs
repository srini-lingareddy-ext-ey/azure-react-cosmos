using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IArtifactFingerprintRepository
{
    Task<ArtifactFingerprint?> GetByArtifactIdAsync(string artifactId, string tenantId, CancellationToken cancellationToken = default);
    Task<ArtifactFingerprint> UpsertAsync(ArtifactFingerprint entity, CancellationToken cancellationToken = default);
}

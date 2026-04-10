using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class ArtifactFingerprintRepository : IArtifactFingerprintRepository
{
    private readonly IRepository<ArtifactFingerprint> _repository;
    public ArtifactFingerprintRepository(IRepository<ArtifactFingerprint> repository) { _repository = repository; }

    public Task<ArtifactFingerprint?> GetByArtifactIdAsync(string artifactId, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(artifactId, tenantId, cancellationToken);

    public Task<ArtifactFingerprint> UpsertAsync(ArtifactFingerprint entity, CancellationToken cancellationToken = default) =>
        _repository.UpsertAsync(entity, cancellationToken);
}

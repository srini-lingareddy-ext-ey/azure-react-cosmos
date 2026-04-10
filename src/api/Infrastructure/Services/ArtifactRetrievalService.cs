using Todo.Api.Application.Services;
using Todo.Api.Domain.Entities;

namespace Todo.Api.Infrastructure.Services;

public sealed class ArtifactRetrievalService : IArtifactRetrievalService
{
    private readonly ILogger<ArtifactRetrievalService> _logger;

    public ArtifactRetrievalService(ILogger<ArtifactRetrievalService> logger) { _logger = logger; }

    public Task<string> RetrieveCanonicalContentAsync(MonitoredArtifact artifact, CancellationToken ct)
    {
        _logger.LogDebug("Retrieving canonical content for artifact {ArtifactId} via connector {ConnectorId}", artifact.Id, artifact.ConnectorId);
        return Task.FromResult(string.Empty);
    }
}

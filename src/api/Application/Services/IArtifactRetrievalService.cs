using Todo.Api.Domain.Entities;

namespace Todo.Api.Application.Services;

public interface IArtifactRetrievalService
{
    Task<string> RetrieveCanonicalContentAsync(MonitoredArtifact artifact, CancellationToken ct);
}

using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class FingerprintApprovedWindowRepository : IFingerprintApprovedWindowRepository
{
    private readonly IRepository<FingerprintApprovedWindow> _repository;
    public FingerprintApprovedWindowRepository(IRepository<FingerprintApprovedWindow> repository) { _repository = repository; }

    public async IAsyncEnumerable<FingerprintApprovedWindow> GetActiveWindowsForArtifactAsync(string artifactId, ArtifactType artifactType, string tenantId, DateTimeOffset at, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId AND c.startTime <= @at AND c.endTime >= @at",
            new Dictionary<string, object> { ["@tenantId"] = tenantId, ["@at"] = at });
        await foreach (var window in _repository.QueryAsync(spec, cancellationToken).ConfigureAwait(false))
        {
            if (window.ScopeType == WindowScopeType.All)
                yield return window;
            else if (window.ScopeType == WindowScopeType.Artifact && string.Equals(window.ScopeValue, artifactId, StringComparison.OrdinalIgnoreCase))
                yield return window;
            else if (window.ScopeType == WindowScopeType.ArtifactType && string.Equals(window.ScopeValue, artifactType.ToString(), StringComparison.OrdinalIgnoreCase))
                yield return window;
        }
    }

    public IAsyncEnumerable<FingerprintApprovedWindow> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId ORDER BY c.startTime DESC",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public Task<FingerprintApprovedWindow> CreateAsync(FingerprintApprovedWindow entity, CancellationToken cancellationToken = default) =>
        _repository.CreateAsync(entity, cancellationToken);

    public Task DeleteAsync(string id, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.DeleteAsync(id, tenantId, null, cancellationToken);
}

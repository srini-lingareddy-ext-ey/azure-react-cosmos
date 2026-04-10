using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed notification template repository (WO-65). Partition key /tenantId. Falls back to platform default.</summary>
public sealed class NotificationTemplateRepository : INotificationTemplateRepository
{
    private readonly IRepository<NotificationTemplate> _repository;
    public NotificationTemplateRepository(IRepository<NotificationTemplate> repository) { _repository = repository; }

    public async Task<NotificationTemplate?> GetByClassificationAsync(string tenantId, string classification, CancellationToken ct = default)
    {
        // Try tenant-specific template first
        var tenantSpec = new QuerySpec(
            "SELECT * FROM c WHERE c.tenantId = @tenantId AND c.classification = @classification",
            new Dictionary<string, object> { ["@tenantId"] = tenantId, ["@classification"] = classification });
        await foreach (var t in _repository.QueryAsync(tenantSpec, ct).ConfigureAwait(false))
            return t;

        // Fall back to platform default
        if (!string.Equals(tenantId, "platform", StringComparison.OrdinalIgnoreCase))
        {
            var platformSpec = new QuerySpec(
                "SELECT * FROM c WHERE c.tenantId = @tenantId AND c.classification = @classification",
                new Dictionary<string, object> { ["@tenantId"] = "platform", ["@classification"] = classification });
            await foreach (var t in _repository.QueryAsync(platformSpec, ct).ConfigureAwait(false))
                return t;
        }

        return null;
    }

    public Task<NotificationTemplate> UpsertAsync(NotificationTemplate template, CancellationToken ct = default) =>
        _repository.UpsertAsync(template, ct);
}

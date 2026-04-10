using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed notification channel repository (WO-65). Partition key /tenantId.</summary>
public sealed class NotificationChannelRepository : INotificationChannelRepository
{
    private readonly IRepository<NotificationChannel> _repository;
    public NotificationChannelRepository(IRepository<NotificationChannel> repository) { _repository = repository; }

    public Task<NotificationChannel?> GetByIdAsync(string id, string tenantId, CancellationToken ct = default) =>
        _repository.GetByIdAsync(id, tenantId, ct);

    public IAsyncEnumerable<NotificationChannel> GetAllByTenantAsync(string tenantId, CancellationToken ct = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, ct);
    }

    public IAsyncEnumerable<NotificationChannel> GetAllEnabledByTenantAsync(string tenantId, CancellationToken ct = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId AND c.isEnabled = true",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, ct);
    }

    public Task<NotificationChannel> CreateAsync(NotificationChannel channel, CancellationToken ct = default) =>
        _repository.CreateAsync(channel, ct);

    public Task<NotificationChannel> UpdateAsync(NotificationChannel channel, CancellationToken ct = default) =>
        _repository.UpdateAsync(channel, ct);

    public Task DeleteAsync(string id, string tenantId, string? etag = null, CancellationToken ct = default) =>
        _repository.DeleteAsync(id, tenantId, etag, ct);
}

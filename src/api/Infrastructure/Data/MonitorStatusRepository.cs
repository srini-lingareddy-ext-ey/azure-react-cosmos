using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed monitor status repository (WO-19). Partition key /tenantId.</summary>
public sealed class MonitorStatusRepository : IMonitorStatusRepository
{
    private readonly IRepository<MonitorStatus> _repository;
    public MonitorStatusRepository(IRepository<MonitorStatus> repository) { _repository = repository; }

    public Task<MonitorStatus?> GetByMonitorIdAsync(string monitorId, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(monitorId, tenantId, cancellationToken);

    public async Task<MonitorStatus> UpsertAsync(MonitorStatus status, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(status.Id, status.TenantId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            status.Etag = existing.Etag;
            return await _repository.UpdateAsync(status, cancellationToken).ConfigureAwait(false);
        }
        return await _repository.CreateAsync(status, cancellationToken).ConfigureAwait(false);
    }
}

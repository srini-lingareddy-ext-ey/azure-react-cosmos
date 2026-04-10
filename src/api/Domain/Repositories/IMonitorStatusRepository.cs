using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

/// <summary>Persistence for <see cref="MonitorStatus"/> (WO-19).</summary>
public interface IMonitorStatusRepository
{
    Task<MonitorStatus?> GetByMonitorIdAsync(string monitorId, string tenantId, CancellationToken cancellationToken = default);
    Task<MonitorStatus> UpsertAsync(MonitorStatus status, CancellationToken cancellationToken = default);
}

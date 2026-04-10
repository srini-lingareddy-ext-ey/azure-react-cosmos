using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IIncidentRepository
{
    Task<IncidentRecord?> GetByIdAsync(string id, string tenantId, CancellationToken ct = default);
    IAsyncEnumerable<IncidentRecord> GetByTenantAsync(string tenantId, string? severity, string? state, DateTimeOffset? from, DateTimeOffset? to, string? sort, string? order, int limit = 50, int offset = 0, CancellationToken ct = default);
    Task<int> CountByTenantAsync(string tenantId, string? severity, string? state, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);
    Task<IncidentRecord> CreateAsync(IncidentRecord record, CancellationToken ct = default);
    Task<IncidentRecord> UpdateAsync(IncidentRecord record, CancellationToken ct = default);
    Task<IncidentRecord?> GetOpenByMonitorAsync(string monitorId, string tenantId, DateTimeOffset since, CancellationToken ct = default);
}
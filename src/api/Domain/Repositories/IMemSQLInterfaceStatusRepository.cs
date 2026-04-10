using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IMemSQLInterfaceStatusRepository
{
    IAsyncEnumerable<MemSQLInterfaceStatus> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<MemSQLInterfaceStatus> UpsertAsync(MemSQLInterfaceStatus entity, CancellationToken cancellationToken = default);
}

using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class MemSQLInterfaceStatusRepository : IMemSQLInterfaceStatusRepository
{
    private readonly IRepository<MemSQLInterfaceStatus> _repository;
    public MemSQLInterfaceStatusRepository(IRepository<MemSQLInterfaceStatus> repository) { _repository = repository; }

    public IAsyncEnumerable<MemSQLInterfaceStatus> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public Task<MemSQLInterfaceStatus> UpsertAsync(MemSQLInterfaceStatus entity, CancellationToken cancellationToken = default) =>
        _repository.UpsertAsync(entity, cancellationToken);
}

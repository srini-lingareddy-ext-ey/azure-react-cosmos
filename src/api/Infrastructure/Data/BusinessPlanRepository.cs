using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed business plan repository (WO-15). Partition key /tenantId.</summary>
public sealed class BusinessPlanRepository : IBusinessPlanRepository
{
    private readonly IRepository<BusinessPlan> _repository;
    public BusinessPlanRepository(IRepository<BusinessPlan> repository) { _repository = repository; }

    public Task<BusinessPlan?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(id, tenantId, cancellationToken);

    public async Task<BusinessPlan?> GetByNameAsync(string name, string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.name = @name AND c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@name"] = name, ["@tenantId"] = tenantId });
        await foreach (var row in _repository.QueryAsync(spec, cancellationToken).ConfigureAwait(false))
            return row;
        return null;
    }

    public IAsyncEnumerable<BusinessPlan> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        var spec = new QuerySpec("SELECT * FROM c WHERE c.tenantId = @tenantId",
            new Dictionary<string, object> { ["@tenantId"] = tenantId });
        return _repository.QueryAsync(spec, cancellationToken);
    }

    public async Task<BusinessPlan> CreateAsync(BusinessPlan businessPlan, CancellationToken cancellationToken = default)
    {
        if (businessPlan.SchemaVersion == 0) businessPlan.SchemaVersion = 1;
        return await _repository.CreateAsync(businessPlan, cancellationToken).ConfigureAwait(false);
    }

    public Task<BusinessPlan> UpdateAsync(BusinessPlan businessPlan, CancellationToken cancellationToken = default) =>
        _repository.UpdateAsync(businessPlan, cancellationToken);
}

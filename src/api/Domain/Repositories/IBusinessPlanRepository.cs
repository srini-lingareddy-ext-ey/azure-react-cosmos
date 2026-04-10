using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

/// <summary>Persistence for <see cref="BusinessPlan"/> (WO-15).</summary>
public interface IBusinessPlanRepository
{
    Task<BusinessPlan?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    Task<BusinessPlan?> GetByNameAsync(string name, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<BusinessPlan> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<BusinessPlan> CreateAsync(BusinessPlan businessPlan, CancellationToken cancellationToken = default);
    Task<BusinessPlan> UpdateAsync(BusinessPlan businessPlan, CancellationToken cancellationToken = default);
}

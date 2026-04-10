using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

/// <summary>Persistence for <see cref="PipelineRegistration"/> (WO-15).</summary>
public interface IPipelineRegistrationRepository
{
    Task<PipelineRegistration?> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    Task<PipelineRegistration?> GetByNameAsync(string name, string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<PipelineRegistration> GetAllByTenantAsync(string tenantId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<PipelineRegistration> GetByBusinessPlanAsync(string businessPlanId, string tenantId, CancellationToken cancellationToken = default);
    Task<PipelineRegistration> CreateAsync(PipelineRegistration registration, CancellationToken cancellationToken = default);
    Task<PipelineRegistration> UpdateAsync(PipelineRegistration registration, CancellationToken cancellationToken = default);
}

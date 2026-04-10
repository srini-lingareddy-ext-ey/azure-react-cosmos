using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IInfraThresholdConfigRepository
{
    Task<InfraThresholdConfig?> GetByComponentIdAsync(string componentId, string tenantId, CancellationToken cancellationToken = default);
    Task<InfraThresholdConfig> UpsertAsync(InfraThresholdConfig entity, CancellationToken cancellationToken = default);
}

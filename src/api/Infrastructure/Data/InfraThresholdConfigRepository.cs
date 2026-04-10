using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

public sealed class InfraThresholdConfigRepository : IInfraThresholdConfigRepository
{
    private readonly IRepository<InfraThresholdConfig> _repository;
    public InfraThresholdConfigRepository(IRepository<InfraThresholdConfig> repository) { _repository = repository; }

    public Task<InfraThresholdConfig?> GetByComponentIdAsync(string componentId, string tenantId, CancellationToken cancellationToken = default) =>
        _repository.GetByIdAsync(componentId, tenantId, cancellationToken);

    public Task<InfraThresholdConfig> UpsertAsync(InfraThresholdConfig entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(entity.Id)) entity.Id = entity.ComponentId;
        return _repository.UpsertAsync(entity, cancellationToken);
    }
}

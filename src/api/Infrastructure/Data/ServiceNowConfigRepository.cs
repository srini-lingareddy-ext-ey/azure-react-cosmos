using Todo.Api.Domain.Entities;
using Todo.Api.Domain.Repositories;

namespace Todo.Api.Infrastructure.Data;

/// <summary>Cosmos-backed ServiceNow config repository (WO-64). Partition key /tenantId. Id = tenantId.</summary>
public sealed class ServiceNowConfigRepository : IServiceNowConfigRepository
{
    private readonly IRepository<ServiceNowIntegrationConfig> _repository;
    public ServiceNowConfigRepository(IRepository<ServiceNowIntegrationConfig> repository) { _repository = repository; }

    public Task<ServiceNowIntegrationConfig?> GetByTenantIdAsync(string tenantId, CancellationToken ct = default) =>
        _repository.GetByIdAsync(tenantId, tenantId, ct);

    public Task<ServiceNowIntegrationConfig> UpsertAsync(ServiceNowIntegrationConfig config, CancellationToken ct = default) =>
        _repository.UpsertAsync(config, ct);
}

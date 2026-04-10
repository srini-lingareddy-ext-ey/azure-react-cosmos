using Todo.Api.Domain.Entities;

namespace Todo.Api.Domain.Repositories;

public interface IServiceNowConfigRepository
{
    Task<ServiceNowIntegrationConfig?> GetByTenantIdAsync(string tenantId, CancellationToken ct = default);
    Task<ServiceNowIntegrationConfig> UpsertAsync(ServiceNowIntegrationConfig config, CancellationToken ct = default);
}
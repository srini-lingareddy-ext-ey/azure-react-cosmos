using Todo.Api.Application.Transport;

namespace Todo.Api.Application.Services;

/// <summary>WO-42: business plan CRUD and lifecycle.</summary>
public interface IBusinessPlanService
{
    Task<BusinessPlanListResponse> ListAsync(string tenantId, bool? isActive, CancellationToken cancellationToken = default);
    Task<BusinessPlanResponse> GetByIdAsync(string id, string tenantId, CancellationToken cancellationToken = default);
    Task<BusinessPlanResponse> CreateAsync(string userId, string tenantId, CreateBusinessPlanRequest request, CancellationToken cancellationToken = default);
    Task<BusinessPlanResponse> UpdateAsync(string userId, string id, string tenantId, UpdateBusinessPlanRequest request, CancellationToken cancellationToken = default);
    Task<BusinessPlanResponse> ActivateAsync(string userId, string id, string tenantId, CancellationToken cancellationToken = default);
    Task<BusinessPlanResponse> DeactivateAsync(string userId, string id, string tenantId, CancellationToken cancellationToken = default);
}
